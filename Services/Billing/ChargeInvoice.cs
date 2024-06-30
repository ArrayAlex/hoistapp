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
            while (!stoppingToken.IsCancellationRequested)
            {
                LogWithInstanceId("Starting to charge invoices...");
                await ChargeInvoicesAsync(stoppingToken);

                LogWithInstanceId("Waiting for 10 minutes before next run...");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Adjust the interval as needed
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
                LogWithInstanceId($"Lock released successfully for {lockKey}.");
            }
            else
            {
                LogWithInstanceId($"Lock token mismatch, lock not released for {lockKey}.");
            }
        }

        private async Task ChargeInvoicesAsync(CancellationToken stoppingToken)
        {
            LogWithInstanceId("Charging invoices...");

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

                    LogWithInstanceId($"{dueInvoices.Count} due invoices found.");

                    foreach (var invoice in dueInvoices)
                    {
                        var lockKey = $"{_lockKeyPrefix}{invoice.InvoiceID}";
                        var lockToken = Guid.NewGuid().ToString();

                        if (await AcquireLockAsync(lockKey, lockToken))
                        {
                            try
                            {
                                LogWithInstanceId($"Processing invoice {invoice.InvoiceID} for company {invoice.CompanyID}...");
                                var tenantDbContext = await tenantDbContextResolver.GetTenantLoginDbContextAsync(invoice.CompanyID);
                                if (tenantDbContext == null)
                                {
                                    LogWithInstanceId($"Tenant DbContext not available for tenant {invoice.CompanyID}.");
                                    continue;
                                }

                                var paymentMethod = await tenantDbContext.paymentgateway.FirstOrDefaultAsync(pm => pm.Active && pm.Default, stoppingToken);
                                if (paymentMethod == null)
                                {
                                    LogWithInstanceId($"Default payment method not available for tenant {invoice.CompanyID}.");
                                    continue;
                                }

                                var charge = await stripeService.CreatePaymentIntentAsync(paymentMethod.CustomerId, invoice.Amount, paymentMethod.MethodId);
                                invoice.Status = "Paid";
                                invoice.IsPaid = true;
                                dbContext.Update(invoice); // Update the invoice status in the master database
                                await dbContext.SaveChangesAsync(stoppingToken);
                                LogWithInstanceId($"Invoice {invoice.InvoiceID} paid successfully.");
                            }
                            catch (Exception ex) when (ex is MySqlException mysqlEx && mysqlEx.Message.Contains("doesn't exist"))
                            {
                                LogWithInstanceId($"Table 'paymentgateway' does not exist for tenant {invoice.CompanyID}. {mysqlEx.Message}");
                            }
                            catch (StripeException ex)
                            {
                                LogWithInstanceId($"Payment failed for invoice {invoice.InvoiceID}. {ex.Message}");
                            }
                            finally
                            {
                                await ReleaseLockAsync(lockKey, lockToken);
                            }
                        }
                        else
                        {
                            LogWithInstanceId($"Could not acquire lock for invoice {invoice.InvoiceID}, another instance might be processing it.");
                        }

                        if (stoppingToken.IsCancellationRequested)
                        {
                            LogWithInstanceId("Cancellation requested, stopping processing.");
                            break;
                        }
                    }

                    LogWithInstanceId("Invoices charged successfully.");
                }
                catch (Exception ex)
                {
                    LogWithInstanceId($"Error charging invoices. {ex.Message}");
                }
            }
        }

        private void LogWithInstanceId(string message)
        {
            System.Diagnostics.Trace.WriteLine($"{_instanceId}: {message}");
        }
    }
}
