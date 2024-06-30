
using hoistmt.Models;
using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
        [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;

        public InvoicesController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
        {
            _tenantDbContextResolver = tenantDbContextResolver;

        }


        [HttpGet("Invoices")]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices([FromQuery] int? id)
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
                    // Retrieve a single invoice by ID along with its related entries
                    var invoices = await dbContext.invoices.FirstOrDefaultAsync(i => i.invoice_id == id.Value);
                        //.Include(i => i.Entries) // Include related entries
                        

                    if (invoices == null)
                    {
                        return Ok("Invoice not found.");
                    }

                    return Ok(invoices);
                }

                // Retrieve all Invoices along with their related entries
                var Invoices = await dbContext.invoices.ToListAsync();
                //.Include(i => i.Entries) // Include related entries
                        

                return Ok(Invoices);
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Trace.WriteLine($"Error fetching Invoices: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpGet("Invoices/ByVehicle")]
        public async Task<ActionResult<Invoice>> getInvoiceByVehicleId([FromQuery] int invoiceId)
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
                var invoices = await dbContext.vehicles.FindAsync(invoiceId);
                if (invoices == null)
                {
                    return NotFound("Vehicle not found.");
                }

                // Once you have the vehicle, get the associated customer
                var customer = await dbContext.invoices.FindAsync(invoices.customerid);
                if (customer == null)
                {
                    return Ok("Customer not found for the specified vehicle.");
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Trace.WriteLine($"Error fetching Customer by Vehicle ID: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("update")]
        public async Task<bool> UpdateInvoice([FromQuery] string token, Invoice Invoice)
        {
            try
            {
                // Get the application db context
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();

                // Update the Invoice in the database
                var invoiceEntity = await dbContext.invoices.FindAsync(Invoice.invoice_id);
                if (invoiceEntity == null)
                {
                    // Invoice not found, return false
                    return false;
                }

                // Update properties of the Invoice entity
                invoiceEntity.total_amount = Invoice.total_amount;
                invoiceEntity.payment_status = Invoice.payment_status;
                invoiceEntity.customerid = Invoice.customerid;

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
        public async Task<bool> AddInvoice([FromQuery] string token, Invoice invoice)
        {
            try
            {
                // Get the application db context
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();

                // Create a new Customer entity
                var invoiceEntity = new Invoice
                {
                    customerid = invoice.invoice_id,
                    invoice_date = DateOnly.FromDateTime(DateTime.Now),
                    total_amount = invoice.total_amount,
                    payment_status = invoice.payment_status,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                // Add the new Customer to the Invoices DbSet
                dbContext.invoices.Add(invoiceEntity);

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

        [HttpDelete("delete/{InvoiceID}")]
        public async Task<bool> DeleteInvoice([FromQuery] string token, int InvoiceID)
        {
            try
            {
                // Get the application db context
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();

                // Find the Customer to delete
                var Invoice = await dbContext.invoices.FindAsync(InvoiceID);
                if (Invoice == null)
                {
                    // Customer not found, return false
                    return false;
                }

                // Set Active to 0 to mark it as deleted


                // Save changes to the database
                dbContext.invoices.Remove(Invoice);
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