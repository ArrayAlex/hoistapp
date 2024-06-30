using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using hoistmt.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace hoistmt.Services.Billing
{
    public class DatabaseKeepAliveService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseKeepAliveService> _logger;

        public DatabaseKeepAliveService(IServiceProvider serviceProvider, ILogger<DatabaseKeepAliveService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running database keep-alive...");
                await KeepDatabaseAliveAsync<MasterDbContext>(stoppingToken);
                await KeepDatabaseAliveAsync<TenantDbContext>(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Adjust the interval as needed
            }
        }

        private async Task KeepDatabaseAliveAsync<TContext>(CancellationToken stoppingToken) where TContext : DbContext
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

                try
                {
                    await dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
                    _logger.LogInformation($"{typeof(TContext).Name} keep-alive query executed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error executing {typeof(TContext).Name} keep-alive query.");
                }
            }
        }
    }
}