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
    public class CustomerController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;

        public CustomerController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
        {
            _tenantDbContextResolver = tenantDbContextResolver;

        }


        [HttpGet("Customers")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers([FromQuery] int? id)
        {
            // Use TenantDbContextResolver to get the tenant-specific DbContext
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return NotFound("Tenant DbContext not available for the retrieved database.");
            }

            try
            {
                if (id.HasValue)
                {
                    // Retrieve a single customer by ID
                    var customer = await dbContext.customers.FindAsync(id.Value);
                    if (customer == null)
                    {
                        return Ok("Customer not found.");
                    }

                    return Ok(new List<Customer> { customer });
                }
                else
                {
                    // Retrieve all customers
                    var customers = await dbContext.customers.ToListAsync();
                    return Ok(customers);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error fetching Customers: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("Customers/ByVehicle")]
        public async Task<ActionResult<Customer>> GetCustomerByVehicleId([FromQuery] int vehicleId)
        {
            // Use TenantDbContextResolver to get the tenant-specific DbContext
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return NotFound("Tenant DbContext not available for the retrieved database.");
            }

            try
            {
                // Find the vehicle by ID
                var vehicle = await dbContext.vehicles.FindAsync(vehicleId);
                if (vehicle == null)
                {
                    return NotFound("Vehicle not found.");
                }

                // Once you have the vehicle, get the associated customer
                var customer = await dbContext.customers.FindAsync(vehicle.customerid);
                if (customer == null)
                {
                    return Ok("Customer not found for the specified vehicle.");
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error fetching Customer by Vehicle ID: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }




        [HttpPut("update")]
        public async Task<bool> UpdateCustomer([FromQuery] string token, Customer Customer)
        {
            try
            {


                // Get the application db context
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();

                // Update the Customer in the database
                var CustomerEntity = await dbContext.customers.FindAsync(Customer.id);
                if (CustomerEntity == null)
                {
                    // Customer not found, return false
                    return false;
                }


                // Update properties of the Customer entity
                CustomerEntity.FirstName = Customer.FirstName;
                CustomerEntity.LastName = Customer.LastName;
                CustomerEntity.Email = Customer.Email;
                CustomerEntity.Phone = Customer.Phone;
                

                // Save changes to the database
                await dbContext.SaveChangesAsync();
                // If update succeeds without exceptions, return true
                return true;
            }
            catch (Exception ex)
            {
                // If any exceptions occur during update, return false
                return false;
            }
        }

        [HttpPost("add")]
        public async Task<bool> AddCustomer([FromQuery] string token, Customer Customer)
        {
            try
            {
                // Get the application db context
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();

                // Create a new Customer entity
                var CustomerEntity = new Customer
                {
                    FirstName = Customer.FirstName,
                    LastName = Customer.LastName,
                    Email = Customer.Email,
                    Phone = Customer.Phone,
                };

                // Add the new Customer to the Customers DbSet
                dbContext.customers.Add(CustomerEntity);

                // Save changes to the database
                await dbContext.SaveChangesAsync();

                // If insertion succeeds without exceptions, return true
                return true;
            }
            catch (Exception ex)
            {
                // If any exceptions occur during insertion, return false
                return false;
            }
        }

        [HttpDelete("delete/{CustomerId}")]
        public async Task<bool> DeleteCustomer([FromQuery] string token, int CustomerId)
        {
            try
            {
                // Get the application db context
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();

                // Find the Customer to delete
                var Customer = await dbContext.customers.FindAsync(CustomerId);
                if (Customer == null)
                {
                    // Customer not found, return false
                    return false;
                }

                // Set Active to 0 to mark it as deleted
              

                // Save changes to the database
                dbContext.customers.Remove(Customer);
                await dbContext.SaveChangesAsync();

                // If deletion succeeds without exceptions, return true
                return true;
            }
            catch (Exception ex)
            {
                // If any exceptions occur during deletion, return false
                return false;
            }
        }
    }
}