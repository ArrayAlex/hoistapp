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
    if (token == null || databaseName == null)
    {
        return BadRequest("Token and database name are required.");
    }
    try
    {
        await _tenantService.VerifyEmail(token, databaseName);
        
        var successHtml = @"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Email Verification</title>
            <style>
                body { font-family: Arial, sans-serif; background-color: #f4f4f9; margin: 0; padding: 0; }
                .container { max-width: 600px; margin: 50px auto; padding: 20px; background-color: #fff; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); }
                h1 { color: #4CAF50; }
                p { font-size: 1.1em; color: #333; }
            </style>
        </head>
        <body>
            <div class='container'>
                <h1>Email Verified Successfully!</h1>
                <p>Your email has been verified successfully. You can now use all the features of our application.</p>
            </div>
        </body>
        </html>";

        return Content(successHtml, "text/html");
    }
    catch (Exception ex)
    {
        var errorHtml = $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Email Verification Error</title>
            <style>
                body {{{{ font-family: Arial, sans-serif; background-color: #f4f4f9; margin: 0; padding: 0; }}}}
.container {{{{ max-width: 600px; margin: 50px auto; padding: 20px; background-color: #fff; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); }}}}
h1 {{{{ color: #4CAF50; }}}}
p {{{{ font-size: 1.1em; color: #333; }}}}
            </style>
        </head>
        <body>
            <div class='container'>
                <h1>Error Verifying Email</h1>
                <p>{ex.Message}</p>
            </div>
        </body>
        </html>";

        return Content(errorHtml, "text/html");
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