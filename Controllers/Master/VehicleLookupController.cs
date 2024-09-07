
using hoistmt.Models.httpModels;
using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;

using hoistmt.HttpClients;
using hoistmt.Interfaces;


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

            var result = await _regoSearch.GetDataAsync(rego, HttpContext.Session.GetString("CompanyDb"));

            if (result.data == null)
            {
                if (result.error == "Not enough credits.")
                {
                    return StatusCode(402, result.error); // 402 Payment Required
                }

                return BadRequest(result.error);
            }

            return Ok(result.data);
        }
    }
}