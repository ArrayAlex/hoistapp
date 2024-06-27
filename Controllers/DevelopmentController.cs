using hoistmt.Data;
using hoistmt.Models;
using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using hoistmt.Functions;
using hoistmt.Models.MasterDbModels;
using Microsoft.AspNetCore.Http;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevelopmentController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private readonly Credits _credits;

        public DevelopmentController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, Credits credits)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _credits = credits;
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