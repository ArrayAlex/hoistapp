using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;
using hoistmt.Functions;
using Microsoft.Extensions.Configuration;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

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

        [HttpGet("connection-string")]
        public IActionResult GetConnectionString()
        {
            string connectionString;
            if (string.IsNullOrEmpty(_configuration["COOKIE_DOMAIN"]))
            {
                connectionString = _configuration.GetConnectionString("masterConnectionLocal");
            }
            else
            {
                connectionString = _configuration.GetConnectionString("masterConnectionRemote");
            }

            return Ok(new { ConnectionString = connectionString });
        }

        [HttpGet("ping")]
        public async Task<IActionResult> PingAddresses()
        {
            var pingResults = new List<object>();

            string[] ipAddresses = { "10.0.0.10", "10.0.0.11" };
            foreach (var ipAddress in ipAddresses)
            {
                var ping = new Ping();
                try
                {
                    var reply = await ping.SendPingAsync(ipAddress);
                    pingResults.Add(new
                    {
                        Address = ipAddress,
                        Status = reply.Status.ToString(),
                        RoundtripTime = reply.RoundtripTime
                    });
                }
                catch (PingException ex)
                {
                    pingResults.Add(new
                    {
                        Address = ipAddress,
                        Status = "Error",
                        ErrorMessage = ex.Message
                    });
                }
            }

            return Ok(pingResults);
        }
    }
}
