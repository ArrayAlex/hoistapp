namespace hoistmt.Interfaces;

public interface ITenantDbContextResolver<TContext>
{
    Task<TContext> GetTenantDbContextAsync(string tenant = null);
    Task<TContext> GetTenantLoginDbContextAsync(string companyid); // Add the parameter
}