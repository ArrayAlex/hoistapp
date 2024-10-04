using Microsoft.AspNetCore.Mvc;
using hoistmt.Data;
using hoistmt.Interfaces;
using hoistmt.Models;
using Microsoft.EntityFrameworkCore;
using hoistmt.Services;
using hoistmt.Services.lib;
using SendGrid.Helpers.Errors.Model;
using NotFoundException = hoistmt.Services.NotFoundException;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginModel model)
        {
            try
            {
                Console.WriteLine(model);
                var result = await _authService.Login(model);
                Console.WriteLine(result);
                return Ok(result);
            }
            catch (InvalidRequest ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Error during login: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("verify")]
        public async Task<ActionResult<bool>> VerifyToken()
        {
            try
            {
                var result = await _authService.VerifyToken();
                return Ok(result);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Error during token verification: {ex.Message}");
                return StatusCode(500, "An error occurred during token verification");
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                await _authService.Logout();
                return Ok();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Error during logout: {ex.Message}");
                return StatusCode(500, "An error occurred during logout");
            }
        }
        
        // [HttpOptions("login")]
        // public IActionResult LoginOptions()
        // {
        //     HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
        //     HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
        //     HttpContext.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        //     return Ok();
        // }
    }
}