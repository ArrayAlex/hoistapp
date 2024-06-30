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
        private readonly string _lockKey = "generate-invoice-lock";

        public GenerateInvoiceService(IConnectionMultiplexer redis, IServiceProvider serviceProvider, ILogger<GenerateInvoiceService> logger)
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
                        _logger.LogInformation("Acquired lock, starting to generate invoices...");
                        await GenerateInvoicesAsync();
                    }
                    finally
                    {
                        await ReleaseLockAsync(lockToken);
                        _logger.LogInformation("Released lock after generating invoices.");
                    }
                }
                else
                {
                    _logger.LogInformation("Could not acquire lock, another instance might be running.");
                }

                _logger.LogInformation("Waiting for 10 minutes before next run...");
                await Task.Delay(TimeSpan.FromMinutes(0.1), stoppingToken); // Adjust the interval as needed
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

        private async Task GenerateInvoicesAsync()
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
                        .ToListAsync();

                    foreach (var company in companiesToBill)
                    {
                        var subscription = await dbContext.plansubscriptions
                            .FirstOrDefaultAsync(s => s.id == company.PlanID);

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
                    }

                    await dbContext.SaveChangesAsync();
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
