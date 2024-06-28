
using hoistmt.Models;
using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;

        public VehicleController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
        }
        
        [HttpGet("Vehicles")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles()
        {
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return Unauthorized("Invalid session");
            }

            var vehicles = await dbContext.vehicles.ToListAsync();
            return Ok(vehicles);
        }
        
        [HttpPost("add")]
        public async Task<IActionResult> AddVehicle([FromBody] Vehicle vehicle)
        {
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return Unauthorized("Invalid session");
            }

            dbContext.vehicles.Add(vehicle); // Assuming VehicleModel maps to the Vehicle entity
            await dbContext.SaveChangesAsync();

            return Ok("Vehicle added successfully");
        }
        
        [HttpGet("vehicle/{id}")]
        public async Task<IActionResult> GetVehicleDetails(int id)
        {
            try
            {
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
                if (dbContext == null)
                {
                    return Unauthorized("Invalid session");
                }

                var vehicle = await dbContext.vehicles.FindAsync(id);
                if (vehicle != null)
                {
                    return Ok(vehicle);
                }

                return NotFound(); // Vehicle with the specified ID not found
            }
            catch (Exception ex)
            {
                // If any exceptions occur during the process, return Internal Server Error
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        [HttpGet("vehicles/customer/{customerId}")]
        public async Task<IActionResult> GetVehiclesByCustomerId(int customerId)
        {
            try
            {
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
                if (dbContext == null)
                {
                    return Unauthorized("Invalid session");
                }

                var vehicles = await dbContext.vehicles.Where(v => v.customerid == customerId).ToListAsync();
                if (vehicles != null && vehicles.Any())
                {
                    return Ok(vehicles);
                }

                return NotFound(); // No vehicles found for the specified customer ID
            }
            catch (Exception ex)
            {
                // If any exceptions occur during the process, return Internal Server Error
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        
        [HttpDelete("delete/{vehicleId}")] // Corrected endpoint definition
        public async Task<IActionResult> DeleteVehicle(int vehicleId)
        {
            try
            {
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
                if (dbContext == null)
                {
                    return Unauthorized("Invalid session");
                }

                var vehicle = await dbContext.vehicles.FindAsync(vehicleId);
                if (vehicle == null)
                {
                    return NotFound(); // Vehicle with the specified ID not found
                }

                dbContext.vehicles.Remove(vehicle);
                await dbContext.SaveChangesAsync();

                return Ok("Vehicle deleted successfully");
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
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
                if (dbContext == null)
                {
                    return Unauthorized("Invalid session");
                }

                var existingVehicle = await dbContext.vehicles.FindAsync(vehicle.id);
                
                if (existingVehicle == null)
                {
                    return NotFound(); // Vehicle with the specified ID not found
                }

                existingVehicle.make = vehicle.make;
                existingVehicle.model = vehicle.model;
                existingVehicle.year = vehicle.year;
                existingVehicle.customerid = vehicle.customerid;
                existingVehicle.description = vehicle.description;

                // Check if customer ID is provided
                if (vehicle.customerid != null)
                {
                    // Find the corresponding customer
                    var existingCustomer = await dbContext.customers.FindAsync(vehicle.customerid);
                    if (existingCustomer == null)
                    {
                        return NotFound("Customer with the specified ID not found");
                    }

                    // Update the owner field of the vehicle with customer's first name and last name
                    existingVehicle.owner = existingCustomer.FirstName + " " + existingCustomer.LastName;
                }
    
                await dbContext.SaveChangesAsync();
                return Ok("Vehicle updated successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }
        
    }
}
