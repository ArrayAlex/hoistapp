using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hoistmt.Data;
using hoistmt.Functions;
using hoistmt.Models.MasterDbModels;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace hoistmt.Services.Billing
{
    public class ChargeInvoiceService : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChargeInvoiceService> _logger;
        private readonly string _lockKeyPrefix = "charge-invoice-lock-";
        private readonly string _instanceKeyPrefix = "charge-invoice-instance-";
        private string _instanceId;

        public ChargeInvoiceService(IConnectionMultiplexer redis, IServiceProvider serviceProvider, ILogger<ChargeInvoiceService> logger)
        {
            _redis = redis;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _instanceId = await AcquireInstanceIdAsync();
            if (_instanceId == null)
            {
                _logger.LogError("Failed to acquire an instance ID.");
                return;
            }

            _logger.LogInformation($"Instance {_instanceId} starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting to charge invoices...");
                await ChargeInvoicesAsync(stoppingToken);

                _logger.LogInformation("Waiting for 10 minutes before next run...");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Adjust the interval as needed
            }
        }

        private async Task<string> AcquireInstanceIdAsync()
        {
            var db = _redis.GetDatabase();
            for (int i = 1; i <= 3; i++)
            {
                var instanceKey = $"{_instanceKeyPrefix}{i}";
                if (await db.StringSetAsync(instanceKey, i.ToString(), TimeSpan.FromMinutes(5), When.NotExists))
                {
                    _logger.LogInformation($"Acquired instance ID {i}");
                    return i.ToString();
                }
            }
            return null;
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

        private async Task LogActionAsync(string message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
                var log = new Logs
                {
                    instanceid = _instanceId,
                    message = message
                };
                dbContext.logs.Add(log);
                await dbContext.SaveChangesAsync();
            }
        }

        private async Task ChargeInvoicesAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Charging invoices...");
            await LogActionAsync("Charging invoices started");

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
                    await LogActionAsync($"{dueInvoices.Count} due invoices found");

                    foreach (var invoice in dueInvoices)
                    {
                        var lockKey = $"{_lockKeyPrefix}{invoice.InvoiceID}";
                        var lockToken = Guid.NewGuid().ToString();

                        if (await AcquireLockAsync(lockKey, lockToken))
                        {
                            try
                            {
                                _logger.LogInformation("Processing invoice {InvoiceID} for company {CompanyID}...", invoice.InvoiceID, invoice.CompanyID);
                                await LogActionAsync($"Processing invoice {invoice.InvoiceID} for company {invoice.CompanyID}");

                                var tenantDbContext = await tenantDbContextResolver.GetTenantLoginDbContextAsync(invoice.CompanyID);
                                if (tenantDbContext == null)
                                {
                                    _logger.LogWarning("Tenant DbContext not available for tenant {CompanyID}.", invoice.CompanyID);
                                    await LogActionAsync($"Tenant DbContext not available for tenant {invoice.CompanyID}");
                                    continue;
                                }

                                var paymentMethod = await tenantDbContext.paymentgateway.FirstOrDefaultAsync(pm => pm.Active && pm.Default, stoppingToken);
                                if (paymentMethod == null)
                                {
                                    _logger.LogWarning("Default payment method not available for tenant {CompanyID}.", invoice.CompanyID);
                                    await LogActionAsync($"Default payment method not available for tenant {invoice.CompanyID}");
                                    continue;
                                }

                                var charge = await stripeService.CreatePaymentIntentAsync(paymentMethod.CustomerId, invoice.Amount, paymentMethod.MethodId);
                                invoice.Status = "Paid";
                                invoice.IsPaid = true;
                                dbContext.Update(invoice); // Update the invoice status in the master database
                                await dbContext.SaveChangesAsync(stoppingToken);
                                _logger.LogInformation("Invoice {InvoiceID} paid successfully.", invoice.InvoiceID);
                                await LogActionAsync($"Invoice {invoice.InvoiceID} paid successfully");
                            }
                            catch (Exception ex) when (ex is MySqlException mysqlEx && mysqlEx.Message.Contains("doesn't exist"))
                            {
                                _logger.LogError(mysqlEx, "Table 'paymentgateway' does not exist for tenant {CompanyID}.", invoice.CompanyID);
                                await LogActionAsync($"Table 'paymentgateway' does not exist for tenant {invoice.CompanyID}: {mysqlEx.Message}");
                            }
                            catch (StripeException ex)
                            {
                                _logger.LogError(ex, "Payment failed for invoice {InvoiceID}.", invoice.InvoiceID);
                                await LogActionAsync($"Payment failed for invoice {invoice.InvoiceID}: {ex.Message}");
                            }
                            finally
                            {
                                await ReleaseLockAsync(lockKey, lockToken);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Could not acquire lock for invoice {InvoiceID}, another instance might be processing it.", invoice.InvoiceID);
                            await LogActionAsync($"Could not acquire lock for invoice {invoice.InvoiceID}, another instance might be processing it.");
                        }

                        if (stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogInformation("Cancellation requested, stopping processing.");
                            await LogActionAsync("Cancellation requested, stopping processing.");
                            break;
                        }
                    }

                    _logger.LogInformation("Invoices charged successfully.");
                    await LogActionAsync("Invoices charged successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error charging invoices.");
                    await LogActionAsync($"Error charging invoices: {ex.Message}");
                }
            }
        }
    }
}
