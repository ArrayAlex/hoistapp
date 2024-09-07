using hoistmt.Exceptions;
using hoistmt.Interfaces;
using hoistmt.Services;
using hoistmt.Services.lib;
using Microsoft.AspNetCore.Mvc;

using Invoice = hoistmt.Models.Invoice;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private readonly LibraryInvoiceService _invoiceService;

        public InvoicesController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver,
            LibraryInvoiceService invoiceService)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _invoiceService = invoiceService;
        }


        [HttpGet("Invoices")]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            try
            {
                var Invoices = await _invoiceService.GetInvoicesAsync();

                return Ok(Invoices);
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
                // Log the exception
                System.Diagnostics.Trace.WriteLine($"Error fetching Invoices: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("Invoice")]
        public async Task<IActionResult> GetInvoice([FromQuery] int invoiceId)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoice(invoiceId);
                return Ok(invoice);
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
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [HttpGet("Invoices/ByVehicle")]
        public async Task<ActionResult<Invoice>> GetInvoicesByVehicleId([FromQuery] int vehicleId)
        {
            try
            {
                // Find the vehicle by ID
                var invoices = await _invoiceService.GetInvoicesByVehicleId(vehicleId);
                return Ok(invoices);
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
                // Log the exception
                System.Diagnostics.Trace.WriteLine($"Error fetching Customer by Vehicle ID: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateInvoice([FromBody] Invoice invoice)
        {
            try
            {
                var invoiceEntity = await _invoiceService.UpdateInvoice(invoice);
                return Ok(invoiceEntity); // Optionally return the updated invoice
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
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddInvoice([FromBody] Invoice invoice)
        {
            try
            {
                var invoiceEntity = await _invoiceService.AddInvoice(invoice);
                return Ok(invoiceEntity);
            } catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            } catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            } 
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while processing your request. {ex.Message}");
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteInvoice([FromQuery] int invoiceId)
        {
            try
            {
                await _invoiceService.DeleteInvoice(invoiceId);
                return Ok("Invoice deleted successfully");
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
                return StatusCode(500, $"An error occurred while processing your request. {ex.Message}");
            }
        }
    }
}