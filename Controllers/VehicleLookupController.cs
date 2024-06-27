using hoistmt.Data;
using hoistmt.Models.httpModels;
using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using hoistmt.HttpClients;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleLookupController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private readonly RegoSearch _regoSearch;

        public VehicleLookupController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, RegoSearch regoSearch)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _regoSearch = regoSearch;
        }

        [HttpGet("regoSearch/{rego}")]
        public async Task<ActionResult<RegoData>> LookUpRego(string rego)
        {
            
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return Unauthorized("Invalid session");
            }
            
            var data = await _regoSearch.GetDataAsync(rego);

            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }
    }
}