using Microsoft.AspNetCore.Mvc;
using hoistmt.Data;
using hoistmt.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using hoistmt.Services;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;
        private readonly TokenHandler _tokenHandler; 

        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;

        public AuthController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, ApplicationDbContext context, IConfiguration configuration, JwtService jwtService, TokenHandler tokenHandler)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _context = context;
            _configuration = configuration;
            _jwtService = jwtService;
            _tokenHandler = tokenHandler;
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginModel model)
        {
            if (model.Company == "" || model.Password == "" || model.Username == "")
            {
                return BadRequest("All fields required");
            }
            var dbContext = await _tenantDbContextResolver.GetTenantLoginDbContextAsync(model.Company);
            if (dbContext == null)
            {
                Console.WriteLine("Tenant database context not found");
                return Unauthorized("Tenant database context not found");
            } 

            
            var account = await dbContext.Set<Account>()
                .FirstOrDefaultAsync(a => a.Username == model.Username && a.Password == model.Password);

           
            if (account == null)
            {
                Console.WriteLine("Invalid username or password");
                return Unauthorized("Invalid username or password");
            }

            HttpContext.Session.SetInt32("userid", account.Id);
            HttpContext.Session.SetString("sessionid: ", HttpContext.Session.Id);
            HttpContext.Session.SetString("CompanyDb", model.Company);

            // Create session entry
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
            Console.WriteLine(HttpContext.Session.Id);

            return Ok(new { Token = HttpContext.Session.Id });
        }
        
        [HttpOptions("login")]
        public IActionResult LoginOptions()
        {
            //HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "http://localhost:3000");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
            HttpContext.Response.Headers.Append("Access-Control-Allow-H eaders", "Content-Type, Authorization");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
            return Ok();
        }
        
        [HttpGet("verify")]
        public async Task<ActionResult<bool>> VerifyToken()
        {
            
            // Call the VerifyToken method of TokenHandler service
            //var isValidToken = await _tokenHandler.VerifyToken(token);
            if(HttpContext.Session.GetString("CompanyDb") == null)
            {
                return Unauthorized();
            }
            else
            {
                return Ok(true);
            }
            
           
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
                Console.WriteLine($"Error during logout: {ex.Message}");
                return StatusCode(500, "An error occurred during logout");
            }
        }

    }
}