using hoistmt.Data;
using hoistmt.Models;
using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
using hoistmt.Functions;
using hoistmt.Models.Account;
using hoistmt.Models.MasterDbModels;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private readonly MasterDbContext _context;
        private Credits _credits;

        public AccountController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, MasterDbContext context, Credits credits)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _context = context;
            _credits = credits;
        }
        
        
        [HttpGet("Accounts")]
        public async Task<ActionResult<IEnumerable<UserAccount>>> GetAccounts([FromQuery] string token)
        {
            // Use TenantDbContextResolver to get the tenant-specific DbContext
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return NotFound("Tenant DbContext not available for the retrieved database.");
            }

            // Your logic here to use the token parameter
            var accounts = await dbContext.accounts.ToListAsync(); // Correct casing here
            return Ok(accounts);
        }

        [HttpGet("Account")]
        public async Task<ActionResult<UserAccount>> GetAccountById()
        {
            // Use TenantDbContextResolver to get the tenant-specific DbContext
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return NotFound("Tenant DbContext not available for the retrieved database.");
            }

            // Retrieve the user ID from the session
            int? userId = HttpContext.Session.GetInt32("userid");
            if (userId == null)
            {
                return Unauthorized("User ID not found in session or invalid format.");
            }

            // Retrieve the account from the database based on the user ID
            var account = await dbContext.accounts.FindAsync(userId);
            if (account == null)
            {
                return NotFound("Account not found.");
            }

            // Retrieve the CompanyDb from the session
            var companyDb = HttpContext.Session.GetString("CompanyDb");

            // Create an anonymous object containing both account and CompanyDb
            var response = new
            {
                Account = account,
                CompanyDb = companyDb
            };

            // Return the response with both account and CompanyDb
            return Ok(response);
        }

        [HttpGet("AvailableCredits")]
        public async Task<CreditsDto> GetCredits()
        {
            return await _credits.GetCredits(HttpContext.Session.GetString("CompanyDb"));
        }
    }

    
}