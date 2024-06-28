using hoistmt.Data;
using hoistmt.Models.MasterDbModels;
using System.Linq;
using System.Threading.Tasks;
using hoistmt.Models.Account;
using Microsoft.EntityFrameworkCore;

namespace hoistmt.Functions
{
    public class Credits
    {
        private readonly MasterDbContext _context;
    
        public Credits(MasterDbContext context)
        {
            _context = context;
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
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyID == companyID);
            if (company == null || company.Credits < amount)
            {
                return false;
            }

            company.Credits -= amount;
            await _context.SaveChangesAsync();
            return true;
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