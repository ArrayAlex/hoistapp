using hoistmt.Data;
using hoistmt.Models.MasterDbModels;
using System.Linq;
using System.Threading.Tasks;
using hoistmt.Models.Account;
using hoistmt.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace hoistmt.Functions
{
    public class Credits
    {
        private readonly MasterDbContext _context;
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
    
        public Credits(MasterDbContext context, ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
        {
            _context = context;
            _tenantDbContextResolver = tenantDbContextResolver;
        }

        public bool hasCredits(string companyID)
        {
            var company = _context.Companies.FirstOrDefault(c => c.CompanyID == companyID);
            if (company == null)
            {
                return false;
            }
            return company.Credits > 0;
        }
        
        public async Task<bool> TryDeductCreditsAsync(string companyID, double amount)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyID == companyID);
                    if (company == null || company.Credits < amount)
                    {
                        return false;
                    }

                    company.Credits -= amount;
                    await _context.SaveChangesAsync();

                    // Create and save the transaction
                    var tenantTransaction = new TenantTransactions
                    {
                        TenantId = companyID,
                        Amount = amount,
                        Date = DateTime.UtcNow,
                        Description = "Rego Search",
                        TransactionType = "Deduction",
                        Status = "Completed",
                        PaymentMethod = "Credits",
                        PaymentDate = DateTime.UtcNow
                    };

                    // Resolve the tenant-specific DbContext
                    var tenantDbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
                    if (tenantDbContext == null)
                    {
                        return false;
                    }

                    // Add the transaction to the tenant-specific context
                    tenantDbContext.tenanttransactions.Add(tenantTransaction);
                    await tenantDbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }


        public async Task<CreditsDto> GetCredits(string companyID)
        {
            var account = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyID == companyID);
            if (account == null)
            {
                return null;
            }

            return new CreditsDto { Credits = account.Credits };
        }
    }
}