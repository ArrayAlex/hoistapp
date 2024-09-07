using hoistmt.Data;
using hoistmt.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using hoistmt.Interfaces;

namespace hoistmt.Functions
{
    public class AccountSubscription
    {
        private readonly MasterDbContext _masterDbContext;
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;

        public AccountSubscription(MasterDbContext masterDbContext, ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
        {
            _masterDbContext = masterDbContext;
            _tenantDbContextResolver = tenantDbContextResolver;
        }

        public async Task<bool> HasFreeUserSlot(string companyId)
        {
            // Retrieve the company record
            var company = await _masterDbContext.Companies.FirstOrDefaultAsync(x => x.CompanyID == companyId);
            if (company == null)
            {
                return false;
            }

            // Retrieve the plan record
            var plan = await _masterDbContext.plansubscriptions.FirstOrDefaultAsync(p => p.id == company.PlanID);
            if (plan == null)
            {
                return false;
            }

            // Get the tenant-specific DbContext
            var tenantDbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (tenantDbContext == null)
            {
                return false;
            }

            // Count the number of user accounts
            var userCount = await tenantDbContext.accounts.CountAsync();

            // Check if the number of users is less than MaxUsers
            return userCount < plan.MaxUsers;
        }
    }
}