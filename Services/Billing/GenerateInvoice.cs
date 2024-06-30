using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hoistmt.Data;
using hoistmt.Models.MasterDbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace hoistmt.Services.Billing
{
    public class GenerateInvoiceService : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GenerateInvoiceService> _logger;
        private readonly string _lockKeyPrefix = "generate-invoice-lock-";
        private readonly string _instanceKeyPrefix = "generate-invoice-instance-";
        private string _instanceId;

        public GenerateInvoiceService(IConnectionMultiplexer redis, IServiceProvider serviceProvider, ILogger<GenerateInvoiceService> logger)
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

            _logger.LogInformation($"Instance {_instanceId} acquired and starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting to generate invoices...");
                await GenerateInvoicesAsync(stoppingToken);

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

        private async Task GenerateInvoicesAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Generating invoices...");

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();

                try
                {
                    var today = DateTime.UtcNow.Date;
                    var companiesToBill = await dbContext.Companies
                        .Where(c => c.NextBilling.Date == today)
                        .ToListAsync(stoppingToken);

                    foreach (var company in companiesToBill)
                    {
                        var lockKey = $"{_lockKeyPrefix}{company.CompanyID}";
                        var lockToken = Guid.NewGuid().ToString();

                        if (await AcquireLockAsync(lockKey, lockToken))
                        {
                            try
                            {
                                var subscription = await dbContext.plansubscriptions
                                    .FirstOrDefaultAsync(s => s.id == company.PlanID, stoppingToken);

                                if (subscription == null)
                                {
                                    _logger.LogWarning("No subscription found for PlanID {PlanID}.", company.PlanID);
                                    continue;
                                }

                                var newInvoice = new CompanInvoice
                                {
                                    CompanyID = company.CompanyID,
                                    Amount = subscription.MonthlyCost, // Assuming monthly billing
                                    Status = "Due",
                                    CreatedDate = today,
                                    DueDate = today.AddDays(7) // Setting due date to 7 days from creation
                                };

                                dbContext.companyinvoices.Add(newInvoice);

                                // Update billing dates
                                company.PrevBilling = today;
                                company.NextBilling = today.AddDays(30); // Assuming monthly billing

                                await dbContext.SaveChangesAsync(stoppingToken);
                                _logger.LogInformation("Invoice generated successfully for company {CompanyID}.", company.CompanyID);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error generating invoice for company {CompanyID}.", company.CompanyID);
                            }
                            finally
                            {
                                await ReleaseLockAsync(lockKey, lockToken);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Could not acquire lock for company {CompanyID}, another instance might be generating its invoice.", company.CompanyID);
                        }

                        if (stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogInformation("Cancellation requested, stopping processing.");
                            break;
                        }
                    }

                    _logger.LogInformation("Invoices generated successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating invoices.");
                }
            }
        }
    }
}