
using hoistmt.Data;
using Microsoft.EntityFrameworkCore;

namespace hoistmt.Services;

public class DatabaseInitializer
{
    private readonly MasterDbContext _masterDbContext;
    private readonly TenantDbContext _tenantDbContext;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        MasterDbContext masterDbContext,
        TenantDbContext tenantDbContext,    
        ILogger<DatabaseInitializer> logger)
    {
        _masterDbContext = masterDbContext;
        _tenantDbContext = tenantDbContext;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await InitializeConnectionAsync(_masterDbContext, "MasterDbContext");
        await InitializeConnectionAsync(_tenantDbContext, "TenantDbContext");
    }

    private async Task InitializeConnectionAsync(DbContext context, string contextName)
    {
        try
        {
            // Perform a simple query to warm up the connection
            await context.Database.ExecuteSqlRawAsync("SELECT 1");
            System.Diagnostics.Trace.WriteLine($"{contextName} connection initialized.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"Error initializing {contextName} connection: {ex.Message}");
        }
    }
}