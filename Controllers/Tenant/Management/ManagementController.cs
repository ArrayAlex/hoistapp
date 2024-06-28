using hoistmt.Data;
using hoistmt.Models;
using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;

using hoistmt.Functions;

using hoistmt.Models.Account;

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

            // Create a new UserAccount object
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

        [HttpGet("AvailableCredits")]
        public async Task<CreditsDto> GetCredits()
        {
            return await _credits.GetCredits(HttpContext.Session.GetString("CompanyDb"));
        }
    }
}
