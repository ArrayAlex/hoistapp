using hoistmt.Data;
using hoistmt.Models;
using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using hoistmt.Functions;
using hoistmt.Models.Account;
using hoistmt.Models.Account.Management;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagementController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private readonly MasterDbContext _context;
        private readonly Credits _credits;
        private readonly AccountSubscription _accountSubscription;

        public ManagementController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver,
            MasterDbContext context, Credits credits, AccountSubscription accountSubscription)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _context = context;
            _credits = credits;
            _accountSubscription = accountSubscription;
        }

        [HttpPost("AddAccount")]
        public async Task<ActionResult<UserAccount>> AddAccount([FromBody] UserAccount model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Use TenantDbContextResolver to get the tenant-specific DbContext
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return NotFound("Tenant DbContext not available for the retrieved database.");
            }

            // Check if there is a free user slot
            if (!await _accountSubscription.HasFreeUserSlot(HttpContext.Session.GetString("CompanyDb")))
            {
                return BadRequest("No free user slots available.");
            }

            // Check if there is already an account with the same email
            var existingemail = await dbContext.accounts.FirstOrDefaultAsync(a => a.email == model.email);
            if (existingemail != null)
            {
                return BadRequest("An account with this email already exists.");
            }
            var existingUsername = await dbContext.accounts.FirstOrDefaultAsync(a => a.Username == model.Username);
            if (existingUsername != null)
            {
                return BadRequest("An account with this username already exists.");
            }

            // Create a new UserAccount objectjkk
            var newUserAccount = new UserAccount
            {
                Name = model.Name,
                Password = model.Password,
                contact = model.contact,
                email = model.email,
                Active = model.Active,
                Username = model.Username,
                roleName = model.roleName,
                position = model.position,
                phone = model.phone,
                roleID = model.roleID
            };

            // Add the new account to the database
            dbContext.accounts.Add(newUserAccount);
            await dbContext.SaveChangesAsync();

            return Ok(newUserAccount);
        }
        
        [HttpGet("Accounts")]
        public async Task<ActionResult<IEnumerable<UserAccountDTO>>> GetAccounts()
        {
            // Use TenantDbContextResolver to get the tenant-specific DbContext
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return NotFound("Tenant DbContext not available for the retrieved database.");
            }

            // Fetch all accounts from the accounts table
            var accounts = await dbContext.accounts.ToListAsync();

            // Map UserAccount to UserAccountDTO
            var accountDTOs = accounts.Select(account => new UserAccountDTO
            {
                Id = account.Id,
                Name = account.Name,
                Contact = account.contact,
                Email = account.email,
                Active = account.Active,
                Username = account.Username,
                RoleName = account.roleName,
                Position = account.position,
                Phone = account.phone,
                RoleID = account.roleID
            }).ToList();

            return Ok(accountDTOs);
        }

        [HttpGet("AvailableCredits")]
        public async Task<CreditsDto> GetCredits()
        {
            return await _credits.GetCredits(HttpContext.Session.GetString("CompanyDb"));
        }
    }
}
