
using hoistmt.Models;
using hoistmt.Services;
using hoistmt.Services.lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hoistmt.Exceptions;
using hoistmt.Interfaces;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private readonly IVehicleService  _vehicleService;
        public VehicleController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, VehicleService vehicleService)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _vehicleService = vehicleService;
        }
        
        [HttpGet("vehicles")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles()
        {
            try
            {
                var vehicles = await _vehicleService.GetVehiclesAsync();
                return Ok(vehicles);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        
        [HttpPost("add")]
        public async Task<IActionResult> AddVehicle([FromBody] Vehicle vehicle)
        {
            try
            {
                var addedVehicle = await _vehicleService.AddVehicleAsync(vehicle);
                return CreatedAtAction(nameof(GetVehicles), new { id = addedVehicle.id });
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        
        [HttpGet("vehicle/{id}")]
        public async Task<IActionResult> GetVehicleDetails(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleDetails(id);
                return Ok(vehicle);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // [HttpGet("vehicles/customer/{customerId}")]
        // public async Task<IActionResult> GetVehiclesByCustomerId(int customerId)
        // {
        //     try
        //     {
        //         var vehicles = await _vehicleService.GetVehiclesByCustomerId(customerId);
        //         if (!vehicles.Any())
        //         {
        //             return NotFound($"No vehicles found for customer ID {customerId}");
        //         }
        //         return Ok(vehicles);
        //     }
        //     catch(UnauthorizedException ex)
        //     {
        //         return Unauthorized(ex.Message);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, "An error occurred while processing your request.");
        //     }
        //     
        // }

        
        [HttpDelete("delete/{vehicleId}")] // Corrected endpoint definition
        public async Task<IActionResult> DeleteVehicle(int vehicleId)
        {
            try
            {
                var vehicle = await _vehicleService.DeleteVehicle(vehicleId);
                return Ok("Vehicle deleted successfully");
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateVehicle([FromBody] Vehicle vehicle)
        {
            try
            {
                var updateVehicle = await _vehicleService.UpdateVehicle(vehicle);
                return Ok("Vehicle updated successfully!");

            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
           
        }
        
    }
}
