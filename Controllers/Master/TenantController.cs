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
        
        private readonly MasterDbContext _context;
        private readonly TenantService _tenantService;

    
        public TenantController(MasterDbContext context, TenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }
        
        [HttpPost("register")]
        // /api/Tenant/register
        public async Task<ActionResult<DbTenant>> CreateTenant(newUser newUser)
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
        public async Task<ActionResult<DbTenant>> GetTenant(int id)
        {
            var tenant = await _context.tenants.FindAsync(id);
        
            if (tenant == null)
            {
                return NotFound();
            }
        
            return tenant;
        }
       
    }
    
    

}