using hoistmt.Data;
using hoistmt.Models;
using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevelopmentController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;

        public DevelopmentController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
        {
            _tenantDbContextResolver = tenantDbContextResolver;

        }

        [HttpGet("Customers")]

        [HttpPost("test")]
        public IActionResult Get200()
        {
            // Return 200 OK response
            return Ok();
        }
        
    }
}