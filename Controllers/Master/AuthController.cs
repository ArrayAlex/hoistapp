using Microsoft.AspNetCore.Mvc;
using hoistmt.Data;
using hoistmt.Interfaces;
using hoistmt.Models;
using Microsoft.EntityFrameworkCore;
using hoistmt.Services;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MasterDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;
        private readonly TokenHandler _tokenHandler;
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;

        public AuthController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver,
            MasterDbContext context, IConfiguration configuration, JwtService jwtService, TokenHandler tokenHandler)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _context = context;
            _configuration = configuration;
            _jwtService = jwtService;
            _tokenHandler = tokenHandler;
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Company) || string.IsNullOrEmpty(model.Password) ||
                string.IsNullOrEmpty(model.Username))
            {   
                return BadRequest("All fields are required");
            }

            var dbContext = await _tenantDbContextResolver.GetTenantLoginDbContextAsync(model.Company);
            if (dbContext == null)
            {
                System.Diagnostics.Trace.WriteLine("Tenant database context not found");
                return Unauthorized("Tenant database context not found");
            }

            var account = await dbContext.Set<UserAccount>()
                .FirstOrDefaultAsync(a => a.Username == model.Username && a.Password == model.Password && a.IsVerified == true);

            if (account == null)
            {
                System.Diagnostics.Trace.WriteLine("Invalid username or password"); 
                return Unauthorized("Invalid username or password");
            }

            HttpContext.Session.SetInt32("userid", account.Id);
            HttpContext.Session.SetString("sessionid", HttpContext.Session.Id);
            HttpContext.Session.SetString("CompanyDb", model.Company);

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyID == model.Company);
            if (company == null)
            {
                return Unauthorized("Company not found");
            }

            var plan = await _context.plansubscriptions.FirstOrDefaultAsync(p => p.id == company.PlanID);
            if (plan == null)
            {
                return Unauthorized("Plan not found");
            }

            HttpContext.Session.SetInt32("PlanID", plan.id);
            HttpContext.Session.SetString("PlanName", plan.PlanName);
            HttpContext.Session.SetInt32("StorageLimitGB", plan.StorageLimitGB);
            HttpContext.Session.SetInt32("MaxUsers", plan.MaxUsers);
            HttpContext.Session.SetString("AccessFeatureA", plan.AccessFeatureA.ToString());
            HttpContext.Session.SetString("AccessFeatureB", plan.AccessFeatureB.ToString());
            HttpContext.Session.SetString("AccessFeatureC", plan.AccessFeatureC.ToString());
            HttpContext.Session.SetString("AccessFeatureD", plan.AccessFeatureD.ToString());
            HttpContext.Session.SetString("AccessFeatureE", plan.AccessFeatureE.ToString());

            var session = new Session
            {
                userID = account.Id,
                token = HttpContext.Session.Id,
                ipAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CompanyDb = HttpContext.Session.GetString("CompanyDb")
                // Add any other session properties you need
            };

            _context.sessions.Add(session);
            await _context.SaveChangesAsync();
            System.Diagnostics.Trace.WriteLine("LOGIN SUCESSFUL");
            System.Diagnostics.Trace.WriteLine(HttpContext.Session.Id);
            Console.Write("PlaneName: ");
            System.Diagnostics.Trace.WriteLine(HttpContext.Session.GetString("PlanName"));
            Console.Write("MaxUsers: ");
            System.Diagnostics.Trace.WriteLine(HttpContext.Session.GetInt32("MaxUsers"));

            return Ok(new
            {
                Token = HttpContext.Session.Id,
                Plan = new
                {
                    PlanID = plan.id,
                    PlanName = plan.PlanName,
                    Cost = plan.MonthlyCost,
                    StorageLimitGB = plan.StorageLimitGB,
                    MaxUsers = plan.MaxUsers,
                    AccessFeatureA = plan.AccessFeatureA,
                    AccessFeatureB = plan.AccessFeatureB,
                    AccessFeatureC = plan.AccessFeatureC,
                    AccessFeatureD = plan.AccessFeatureD,
                    AccessFeatureE = plan.AccessFeatureE
                }
            });
        }

        [HttpOptions("login")]
        public IActionResult LoginOptions()
        {
            HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
            return Ok();
        }

        [HttpGet("verify")]
        public async Task<ActionResult<bool>> VerifyToken()
        {
            if (HttpContext.Session.GetString("CompanyDb") == null)
            {
                return Unauthorized();
            }

            return Ok(true);
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                // Clear all session variables
                HttpContext.Session.Clear();

                // Retrieve the session token from HttpContext
                var sessionId = HttpContext.Session.GetString("Token");

                // Set the expiration time of the session to now
                var session = await _context.sessions.FindAsync(sessionId);
                if (session != null)
                {
                    session.ExpiresAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return Ok("Logout successful");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Error during logout: {ex.Message}");
                return StatusCode(500, "An error occurred during logout");
            }
        }
    }
}