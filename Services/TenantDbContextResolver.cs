using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;

namespace hoistmt.Services
{
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

        public async Task<TContext> GetTenantDbContextAsync()
        {
            var tenantSchemaName = _httpContextAccessor.HttpContext.Session.GetString("CompanyDb");
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

            Console.WriteLine("Tenant Schema Name: " + tenantSchemaName);
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

        // You might not need this method anymore, as you are not using IDbContextFactory
        // public void SwitchTenantSchema()
        // {
        //     var tenantSchemaName = _httpContextAccessor.HttpContext.Session.GetString("CompanyDb");
        //     if (tenantSchemaName != null)
        //     {
        //         _dbContext.Database.GetDbConnection().ChangeDatabase(tenantSchemaName); // Switch the schema dynamically
        //     }
        // }
    }

    public interface ITenantDbContextResolver<TContext> where TContext : DbContext
{
    Task<TContext> GetTenantDbContextAsync();
    Task<TContext> GetTenantLoginDbContextAsync(string companyid); // Add the parameter
}

    
}