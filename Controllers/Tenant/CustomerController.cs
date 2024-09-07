
using hoistmt.Exceptions;
using hoistmt.Interfaces;
using hoistmt.Models;
using hoistmt.Services;
using hoistmt.Services.lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private CustomerService _customerSevice;

        public CustomerController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, CustomerService customerService)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _customerSevice = customerService;
        }

        [HttpGet("Customers")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            try
            {
                var customers = await _customerSevice.GetCustomers();
                return Ok(customers);
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
                System.Diagnostics.Trace.WriteLine($"Error fetching Customers: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("Customers/ByVehicle")]
        public async Task<ActionResult<Customer>> GetCustomerByVehicleId([FromQuery] int vehicleId)
        {
            try
            {
                // Find the vehicle by ID
                var customer = await _customerSevice.GetCustomerByVehicleId(vehicleId);
                return Ok(customer);
            } catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Error fetching Customer by Vehicle ID: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
        
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCustomer([FromBody] Customer customer)
        {
            try
            {
                var updateCustomer = await _customerSevice.UpdateCustomer(customer);
                return Ok(updateCustomer);
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
                return StatusCode(500, $"error updating customer: {ex.Message}");
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddCustomer([FromBody] Customer customer)
        {
            try
            {
                var addCustomer = await _customerSevice.AddCustomer(customer);
                return Ok(addCustomer);
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
                return StatusCode(500, $"error adding customer {ex.Message}");
            }
        }

        [HttpDelete("delete/{CustomerId}")]
        public async Task<IActionResult> DeleteCustomer(int CustomerId)
        {
            try
            {
                var deleteCustomer = await _customerSevice.DeleteCustomer(CustomerId);
                return Ok(deleteCustomer);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}