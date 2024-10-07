using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;
using hoistmt.Functions;
using Microsoft.Extensions.Configuration;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using hoistmt.Interfaces;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevelopmentController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private readonly Credits _credits;
        private readonly IConfiguration _configuration;

        public DevelopmentController(
            ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, 
            Credits credits,
            IConfiguration configuration)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _credits = credits;
            _configuration = configuration;
        }

        [HttpGet("Customers")]
        public IActionResult GetCustomers()
        {
            // Implement logic to get customers
            return Ok();
        }

        [HttpGet("test")]
        public IActionResult Get200()
        {
            var companyDb = HttpContext.Session.GetString("CompanyDb");
            if (string.IsNullOrEmpty(companyDb))
            {
                return BadRequest("CompanyDb is missing in session.");
            }

            var hasCredits = _credits.hasCredits(companyDb);
            return Ok(hasCredits);
        }

    }
}
