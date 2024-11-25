using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hoistmt.Models;
using hoistmt.Models.Tenant;
using hoistmt.Services.lib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using hoistmt.Exceptions;
using hoistmt.Services;

namespace hoistmt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly LibraryInvoiceService _invoiceService;
        private readonly TenantDbContext _context;

        public InvoicesController(LibraryInvoiceService invoiceService, TenantDbContext context)
        {
            _invoiceService = invoiceService;
            _context = context;
        }

        // GET: api/invoices
        [HttpGet("invoices")]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            try
            {
                var invoices = await _invoiceService.GetInvoicesAsync();
                return Ok(invoices);
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Invoice>> CreateInvoice([FromBody] InvoiceRequest request)
        {
            if (request?.Invoice == null)
            {
                return BadRequest("Invalid invoice data");
            }

            try
            {
                var invoice = await _invoiceService.CreateInvoiceAsync(request.Invoice);
                return CreatedAtAction(nameof(GetInvoice), new { id = invoice.invoice_id }, invoice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/invoices/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] InvoiceRequest request)
        {
            if (request?.Invoice == null || id != request.Invoice.invoice_id)
            {
                return BadRequest("Invalid invoice data");
            }

            try
            {
                var updatedInvoice = await _invoiceService.UpdateInvoiceAsync(id, request.Invoice);
                return Ok(updatedInvoice);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // GET: api/invoices/5
        [HttpGet("invoice/{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(int id)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceAsync(id);
                return Ok(invoice);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //
        // // PUT: api/invoices/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> UpdateInvoice(int id, [FromBody] Invoice updatedInvoice)
        // {
        //     if (id != updatedInvoice.invoice_id)
        //     {
        //         return BadRequest();
        //     }
        //
        //     try
        //     {
        //         var existingInvoice = await _context.invoices
        //             .Include(i => i.LineItems)
        //             .FirstOrDefaultAsync(i => i.invoice_id == id);
        //
        //         if (existingInvoice == null)
        //         {
        //             return NotFound();
        //         }
        //
        //         // Update invoice properties
        //         existingInvoice.Status = updatedInvoice.Status;
        //         existingInvoice.PaymentTerms = updatedInvoice.PaymentTerms;
        //         existingInvoice.Notes = updatedInvoice.Notes;
        //         existingInvoice.TaxRate = updatedInvoice.TaxRate;
        //         existingInvoice.Discount = updatedInvoice.Discount;
        //         existingInvoice.dueDate = updatedInvoice.dueDate;
        //         existingInvoice.customerid = updatedInvoice.customerid;
        //         existingInvoice.updated_at = DateTime.UtcNow;
        //
        //         // Update line items
        //         _context.LineItems.RemoveRange(existingInvoice.LineItems);
        //         
        //         if (updatedInvoice.LineItems != null)
        //         {
        //             foreach (var item in updatedInvoice.LineItems)
        //             {
        //                 item.invoice_id = id;
        //             }
        //             existingInvoice.LineItems = updatedInvoice.LineItems;
        //         }
        //
        //         // Recalculate totals
        //         existingInvoice.Subtotal = (int)existingInvoice.LineItems?.Sum(li => li.Rate * li.Hours);
        //         existingInvoice.TaxAmount = existingInvoice.TaxRate.HasValue ? 
        //             (int)(existingInvoice.Subtotal * (existingInvoice.TaxRate / 100.0)) : 0;
        //         existingInvoice.DiscountAmount = existingInvoice.Discount ?? 0;
        //         existingInvoice.Total = existingInvoice.Subtotal + existingInvoice.TaxAmount - existingInvoice.DiscountAmount;
        //
        //         await _context.SaveChangesAsync();
        //
        //         return NoContent();
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, $"Internal server error: {ex.Message}");
        //     }
        // }

        // DELETE: api/invoices/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            try
            {
                await _invoiceService.DeleteInvoiceAsync(id);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}