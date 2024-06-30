
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace hoistmt.Services;

public class TenantDbContextResolver<TContext> : ITenantDbContextResolver<TContext>
    where TContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TContext _dbContext;

    public TenantDbContextResolver(
        IHttpContextAccessor httpContextAccessor,
        TContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public async Task<TContext> GetTenantDbContextAsync(string tenant = null)
    {
        var tenantSchemaName = _httpContextAccessor.HttpContext.Session.GetString("CompanyDb");
        if (tenant != null)
        {
            tenantSchemaName = tenant;
        }

        if (tenantSchemaName == null)
        {
            return null;
        }

        // Open connection if not already open
        if (_dbContext.Database.GetDbConnection().State != ConnectionState.Open)
        {
            await _dbContext.Database.GetDbConnection().OpenAsync();
        }

        // Change the schema
        await _dbContext.Database.GetDbConnection().ChangeDatabaseAsync(tenantSchemaName);

        return _dbContext;
    }
    public async Task<TContext> GetTenantLoginDbContextAsync(string companyid)
    {

        
        var tenantSchemaName = companyid;

        System.Diagnostics.Trace.WriteLine("Tenant Schema Name: " + tenantSchemaName);
        if (tenantSchemaName == null)
        {
            return null;
        }

        // Open connection if not already open
        if (_dbContext.Database.GetDbConnection().State != ConnectionState.Open)
        {
            await _dbContext.Database.GetDbConnection().OpenAsync();
        }

        // Change the schema
        await _dbContext.Database.GetDbConnection().ChangeDatabaseAsync(tenantSchemaName);

        return _dbContext;
    }

}

public interface ITenantDbContextResolver<TContext>
{
    Task<TContext> GetTenantDbContextAsync(string tenant = null);
    Task<TContext> GetTenantLoginDbContextAsync(string companyid); // Add the parameter
}