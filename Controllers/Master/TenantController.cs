using Microsoft.AspNetCore.Mvc;
using hoistmt.Data;
using hoistmt.Models;
using hoistmt.Services;
using System;
using System.Threading.Tasks;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly TenantService _tenantService;

        public TenantController(TenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<DbTenant>> CreateTenant(newUser newUser)
        {
            try
            {
                var tenant = await _tenantService.CreateTenant(newUser);
                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating the tenant: {ex.Message}");
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string token, string databaseName)
        {
            if(token == null|| databaseName == null)
            {
                return BadRequest("Token and database name are required.");
            }
            try
            {
                await _tenantService.VerifyEmail(token, databaseName);
                return Ok("Email verified successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset(string email, string databaseName)
        {
            try
            {
                await _tenantService.RequestPasswordReset(email, databaseName);
                return Ok("Password reset email sent.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string databaseName)
        {
            try
            {
                await _tenantService.ResetPassword(token, newPassword, databaseName);
                return Ok("Password reset successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
