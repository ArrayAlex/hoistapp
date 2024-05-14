using Microsoft.AspNetCore.Mvc;
using hoistmt.Data;
using hoistmt.Models;
using hoistmt.Services;
using Microsoft.EntityFrameworkCore;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        
        private readonly ApplicationDbContext _context;
        private readonly TenantService _tenantService;

    
        public TenantController(ApplicationDbContext context, TenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }
        
        [HttpPost("register")]
        // /api/Tenant/register
        public async Task<ActionResult<Tenant>> CreateTenant(newUser newUser)
        {
            try
            {
                if(newUser.DatabaseName == "" || newUser.Name == "" || newUser.email == "" || newUser.Password == ""  || newUser.Username == "")
                {
                    return BadRequest("All fields are required. ");
                }
                {
                    return BadRequest("DatabaseName is required. ");
                }
                var existingTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.DatabaseName == newUser.DatabaseName);
                
                if (existingTenant != null)
                {
                    // DatabaseName is already taken, return appropriate response
                    return Conflict("DatabaseName is already taken. ");
                }
                var createdTenant = await _tenantService.CreateTenant(newUser);
                // CreatedAtAction(nameof(GetTenant), new { id = createdTenant.Id }, createdTenant);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(500, $"An error occurred while creating the tenant. {ex.Message}");
            }
        }
        
        [HttpGet("{id}")] 
        public async Task<ActionResult<Tenant>> GetTenant(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
        
            if (tenant == null)
            {
                return NotFound();
            }
        
            return tenant;
        }
       
    }
    
    

}