using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;
using hoistmt.Data;
using hoistmt.Functions;
using hoistmt.Models.MasterDbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Stripe;

namespace hoistmt.Services.Billing
{
    public class ChargeInvoiceService : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChargeInvoiceService> _logger;
        private readonly string _lockKeyPrefix = "charge-invoice-lock-";
        private readonly string _instanceId;

        public ChargeInvoiceService(IConnectionMultiplexer redis, IServiceProvider serviceProvider, ILogger<ChargeInvoiceService> logger)
        {
            _redis = redis;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? "UnknownInstance";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Log instance ID every 10 seconds
            _ = LogInstanceIdPeriodicallyAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting to charge invoices...");
                await ChargeInvoicesAsync(stoppingToken);

                _logger.LogInformation("Waiting for 10 minutes before next run...");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Adjust the interval as needed
            }
        }

        private async Task LogInstanceIdPeriodicallyAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await LogInstanceIdAsync();
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task LogInstanceIdAsync()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
                    var log = new Logs
                    {
                        instanceid = _instanceId,
                        message = $"Running instance ID: {_instanceId}"
                    };
                    dbContext.logs.Add(log);
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Logged instance ID: {_instanceId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging instance ID to the database.");
            }
        }

        private async Task<bool> AcquireLockAsync(string lockKey, string lockToken)
        {
            var db = _redis.GetDatabase();
            return await db.StringSetAsync(lockKey, lockToken, TimeSpan.FromMinutes(5), When.NotExists);
        }

        private async Task ReleaseLockAsync(string lockKey, string lockToken)
        {
            var db = _redis.GetDatabase();
            var storedLockToken = await db.StringGetAsync(lockKey);
            if (storedLockToken == lockToken)
            {
                await db.KeyDeleteAsync(lockKey);
                _logger.LogInformation("Lock released successfully for {lockKey}.", lockKey);
            }
            else
            {
                _logger.LogInformation("Lock token mismatch, lock not released for {lockKey}.", lockKey);
            }
        }

        private async Task ChargeInvoicesAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Charging invoices...");

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
                var tenantDbContextResolver = scope.ServiceProvider.GetRequiredService<ITenantDbContextResolver<TenantDbContext>>();
                var stripeService = scope.ServiceProvider.GetRequiredService<StripeService>();

                try
                {
                    var today = DateTime.UtcNow.Date;

                    // Fetch due invoices from the master database
                    var dueInvoices = await dbContext.companyinvoices
                        .Where(i => i.Status == "Due" && (i.DueDate.Date == today || i.CreatedDate.Date == today))
                        .ToListAsync(stoppingToken);

                    _logger.LogInformation("{Count} due invoices found.", dueInvoices.Count);

                    foreach (var invoice in dueInvoices)
                    {
                        var lockKey = $"{_lockKeyPrefix}{invoice.InvoiceID}";
                        var lockToken = Guid.NewGuid().ToString();

                        if (await AcquireLockAsync(lockKey, lockToken))
                        {
                            try
                            {
                                _logger.LogInformation("Processing invoice {InvoiceID} for company {CompanyID}...", invoice.InvoiceID, invoice.CompanyID);
                                var tenantDbContext = await tenantDbContextResolver.GetTenantLoginDbContextAsync(invoice.CompanyID);
                                if (tenantDbContext == null)
                                {
                                    _logger.LogWarning("Tenant DbContext not available for tenant {CompanyID}.", invoice.CompanyID);
                                    continue;
                                }

                                var paymentMethod = await tenantDbContext.paymentgateway.FirstOrDefaultAsync(pm => pm.Active && pm.Default, stoppingToken);
                                if (paymentMethod == null)
                                {
                                    _logger.LogWarning("Default payment method not available for tenant {CompanyID}.", invoice.CompanyID);
                                    continue;
                                }

                                var charge = await stripeService.CreatePaymentIntentAsync(paymentMethod.CustomerId, invoice.Amount, paymentMethod.MethodId);
                                invoice.Status = "Paid";
                                invoice.IsPaid = true;
                                dbContext.Update(invoice); // Update the invoice status in the master database
                                await dbContext.SaveChangesAsync(stoppingToken);
                                _logger.LogInformation("Invoice {InvoiceID} paid successfully.", invoice.InvoiceID);
                            }
                            catch (Exception ex) when (ex is MySqlException mysqlEx && mysqlEx.Message.Contains("doesn't exist"))
                            {
                                _logger.LogError(mysqlEx, "Table 'paymentgateway' does not exist for tenant {CompanyID}.", invoice.CompanyID);
                            }
                            catch (StripeException ex)
                            {
                                _logger.LogError(ex, "Payment failed for invoice {InvoiceID}.", invoice.InvoiceID);
                            }
                            finally
                            {
                                await ReleaseLockAsync(lockKey, lockToken);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Could not acquire lock for invoice {InvoiceID}, another instance might be processing it.", invoice.InvoiceID);
                        }

                        if (stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogInformation("Cancellation requested, stopping processing.");
                            break;
                        }
                    }

                    _logger.LogInformation("Invoices charged successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error charging invoices.");
                }
            }
        }
    }
}
