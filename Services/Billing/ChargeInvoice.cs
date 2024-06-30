using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hoistmt.Data;
using hoistmt.Functions;
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
        private readonly string _lockKey = "charge-invoice-lock";

        public ChargeInvoiceService(IConnectionMultiplexer redis, IServiceProvider serviceProvider, ILogger<ChargeInvoiceService> logger)
        {
            _redis = redis;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var lockToken = Guid.NewGuid().ToString();
                if (await AcquireLockAsync(lockToken))
                {
                    try
                    {
                        _logger.LogInformation("Acquired lock, starting to charge invoices...");
                        await ChargeInvoicesAsync();
                    }
                    finally
                    {
                        await ReleaseLockAsync(lockToken);
                        _logger.LogInformation("Released lock after charging invoices.");
                    }
                }
                else
                {
                    _logger.LogInformation("Could not acquire lock, another instance might be running.");
                }

                _logger.LogInformation("Waiting for 10 minutes before next run...");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Adjust the interval as needed
            }
        }

        private async Task<bool> AcquireLockAsync(string lockToken)
        {
            var db = _redis.GetDatabase();
            var acquired = await db.StringSetAsync(_lockKey, lockToken, TimeSpan.FromMinutes(5), When.NotExists);
            if (acquired)
            {
                _logger.LogInformation("Lock acquired successfully.");
            }
            else
            {
                _logger.LogInformation("Failed to acquire lock.");
            }
            return acquired;
        }

        private async Task ReleaseLockAsync(string lockToken)
        {
            var db = _redis.GetDatabase();
            var storedLockToken = await db.StringGetAsync(_lockKey);
            if (storedLockToken == lockToken)
            {
                await db.KeyDeleteAsync(_lockKey);
                _logger.LogInformation("Lock released successfully.");
            }
            else
            {
                _logger.LogInformation("Lock token mismatch, lock not released.");
            }
        }

        private async Task ChargeInvoicesAsync()
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
                        .ToListAsync();
                    
                    _logger.LogInformation($"{dueInvoices.Count} due invoices found.");

                    foreach (var invoice in dueInvoices)
                    {
                        _logger.LogInformation($"Processing invoice {invoice.InvoiceID} for company {invoice.CompanyID}...");
                        var tenantDbContext = await tenantDbContextResolver.GetTenantLoginDbContextAsync(invoice.CompanyID);
                        if (tenantDbContext == null)
                        {
                            _logger.LogWarning($"Tenant DbContext not available for tenant {invoice.CompanyID}.");
                            continue;
                        }

                        try
                        {
                            var paymentMethod = await tenantDbContext.paymentgateway.FirstOrDefaultAsync(pm => pm.Active && pm.Default);
                            if (paymentMethod == null)
                            {
                                _logger.LogWarning($"Default payment method not available for tenant {invoice.CompanyID}.");
                                continue;
                            }

                            var charge = await stripeService.CreatePaymentIntentAsync(paymentMethod.CustomerId, invoice.Amount, paymentMethod.MethodId);
                            invoice.Status = "Paid";
                            invoice.IsPaid = true;
                            dbContext.Update(invoice); // Update the invoice status in the master database
                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation($"Invoice {invoice.InvoiceID} paid successfully.");
                        }
                        catch (Exception ex) when (ex is MySqlException mysqlEx && mysqlEx.Message.Contains("doesn't exist"))
                        {
                            _logger.LogError(mysqlEx, $"Table 'paymentgateway' does not exist for tenant {invoice.CompanyID}.");
                        }
                        catch (StripeException ex)
                        {
                            _logger.LogError(ex, $"Payment failed for invoice {invoice.InvoiceID}.");
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
