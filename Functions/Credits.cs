using hoistmt.Data;
using hoistmt.Models.MasterDbModels;
using System.Linq;

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
    }
}