using Microsoft.EntityFrameworkCore;
using hoistmt.Exceptions;
using hoistmt.Interfaces;
using hoistmt.Models;
using System.Transactions;
using hoistmt.Models.Tenant;

namespace hoistmt.Services.lib;

public class LibraryInvoiceService : IDisposable
{
    private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
    private TenantDbContext _context;
    private bool _disposed = false;

    public LibraryInvoiceService(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
    {
        _tenantDbContextResolver = tenantDbContextResolver ??
                                   throw new ArgumentNullException(nameof(tenantDbContextResolver));
    }

    private async Task EnsureContextInitializedAsync()
    {
        if (_context == null)
        {
            _context = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (_context == null)
            {
                throw new UnauthorizedException("Not logged in or unauthorized access.");
            }
        }
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesAsync()
    {
        await EnsureContextInitializedAsync();

        return await _context.invoices
            .Include(i => i.LineItems)
            .Include(i => i.Customer)
            .OrderByDescending(i => i.created_at)
            .ToListAsync();
    }

    public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        await EnsureContextInitializedAsync();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Set creation timestamps
            var now = DateTime.UtcNow;
            invoice.created_at = now;
            invoice.updated_at = now;

            // Handle line items
            var lineItems = invoice.LineItems?.ToList() ?? new List<LineItem>();
            invoice.LineItems = null; // Temporarily clear to avoid duplicate inserts

            // Add the invoice first
            _context.invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Now add the line items with the new invoice_id
            if (lineItems.Any())
            {
                foreach (var item in lineItems)
                {
                    item.invoice_id = invoice.invoice_id;
                    _context.LineItems.Add(item);
                }

                await _context.SaveChangesAsync();
            }

            // Calculate totals
            await CalculateInvoiceTotals(invoice);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // Reload the invoice with all related data
            return await GetInvoiceAsync(invoice.invoice_id);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Invoice> UpdateInvoiceAsync(int id, Invoice updatedInvoice)
    {
        await EnsureContextInitializedAsync();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingInvoice = await _context.invoices
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.invoice_id == id);

            if (existingInvoice == null)
            {
                throw new NotFoundException($"Invoice with ID {id} not found.");
            }

            // Update basic invoice properties
            existingInvoice.Status = updatedInvoice.Status;
            existingInvoice.PaymentTerms = updatedInvoice.PaymentTerms;
            existingInvoice.Notes = updatedInvoice.Notes;
            existingInvoice.TaxRate = updatedInvoice.TaxRate;
            existingInvoice.Discount = updatedInvoice.Discount;
            existingInvoice.customerid = updatedInvoice.customerid;
            existingInvoice.dueDate = updatedInvoice.dueDate;
            existingInvoice.updated_at = DateTime.UtcNow;

            // Handle line items
            foreach (var updatedItem in updatedInvoice.LineItems ?? Enumerable.Empty<LineItem>())
            {
                var existingItem = existingInvoice.LineItems?
                    .FirstOrDefault(li => li.Id == updatedItem.Id);

                if (existingItem != null)
                {
                    // Update existing line item
                    existingItem.Title = updatedItem.Title;
                    existingItem.Rate = updatedItem.Rate;
                    existingItem.Hours = updatedItem.Hours;
                    existingItem.Type = updatedItem.Type;
                }
                else if (updatedItem.Id == 0)
                {
                    // This is a new line item
                    updatedItem.invoice_id = id;
                    _context.LineItems.Add(updatedItem);
                }
                // If item has an ID but wasn't found, ignore it
            }

            // Remove any line items that are no longer present
            if (existingInvoice.LineItems != null)
            {
                var updatedItemIds = updatedInvoice.LineItems?
                    .Select(li => li.Id)
                    .ToList() ?? new List<long>();

                var itemsToRemove = existingInvoice.LineItems
                    .Where(li => !updatedItemIds.Contains(li.Id))
                    .ToList();

                _context.LineItems.RemoveRange(itemsToRemove);
            }

            // Save changes and recalculate totals
            await _context.SaveChangesAsync();
            await CalculateInvoiceTotals(existingInvoice);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // Reload the invoice with all related data
            return await GetInvoiceAsync(id);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Invoice> GetInvoiceAsync(int id)
    {
        await EnsureContextInitializedAsync();

        var invoice = await _context.invoices
            .Include(i => i.LineItems)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.invoice_id == id);

        if (invoice == null)
        {
            throw new NotFoundException($"Invoice with ID {id} not found.");
        }

        return invoice;
    }

    /*public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        await EnsureContextInitializedAsync();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Set creation timestamps
            var now = DateTime.UtcNow;
            invoice.created_at = now;
            invoice.updated_at = now;

            // Calculate totals
            await CalculateInvoiceTotals(invoice);

            // Add the invoice
            _context.invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Update line items with the new invoice ID
            if (invoice.LineItems != null)
            {
                foreach (var item in invoice.LineItems)
                {
                    item.invoice_id = invoice.invoice_id;
                }
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return invoice;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }*/

    /*public async Task<Invoice> UpdateInvoiceAsync(int id, Invoice updatedInvoice)
    {
        await EnsureContextInitializedAsync();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingInvoice = await _context.invoices
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.invoice_id == id);

            if (existingInvoice == null)
            {
                throw new NotFoundException($"Invoice with ID {id} not found.");
            }

            // Update basic properties
            existingInvoice.Status = updatedInvoice.Status;
            existingInvoice.PaymentTerms = updatedInvoice.PaymentTerms;
            existingInvoice.Notes = updatedInvoice.Notes;
            existingInvoice.TaxRate = updatedInvoice.TaxRate;
            existingInvoice.Discount = updatedInvoice.Discount;
            existingInvoice.customerid = updatedInvoice.customerid;
            existingInvoice.dueDate = updatedInvoice.dueDate;
            existingInvoice.updated_at = DateTime.UtcNow;

            // Update line items
            if (existingInvoice.LineItems != null)
            {
                _context.LineItems.RemoveRange(existingInvoice.LineItems);
            }

            if (updatedInvoice.LineItems != null)
            {
                foreach (var item in updatedInvoice.LineItems)
                {
                    item.invoice_id = id;
                }
                existingInvoice.LineItems = updatedInvoice.LineItems;
            }

            // Recalculate totals
            await CalculateInvoiceTotals(existingInvoice);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return existingInvoice;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    */

    public async Task DeleteInvoiceAsync(int id)
    {
        await EnsureContextInitializedAsync();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var invoice = await _context.invoices
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.invoice_id == id);

            if (invoice == null)
            {
                throw new NotFoundException($"Invoice with ID {id} not found.");
            }

            // Remove line items first
            if (invoice.LineItems != null)
            {
                _context.LineItems.RemoveRange(invoice.LineItems);
            }

            // Remove the invoice
            _context.invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByCustomerAsync(int customerId)
    {
        await EnsureContextInitializedAsync();

        var invoices = await _context.invoices
            .Include(i => i.LineItems)
            .Include(i => i.Customer)
            .Where(i => i.customerid == customerId)
            .OrderByDescending(i => i.created_at)
            .ToListAsync();

        if (!invoices.Any())
        {
            throw new NotFoundException($"No invoices found for customer with ID {customerId}.");
        }

        return invoices;
    }

    private async Task CalculateInvoiceTotals(Invoice invoice)
    {
        if (invoice.LineItems == null)
        {
            invoice.Subtotal = 0;
            invoice.TaxAmount = 0;
            invoice.Total = 0;
            return;
        }

        // Calculate subtotal
        invoice.Subtotal = (int)invoice.LineItems.Sum(li => (li.Rate ?? 0) * (li.Hours ?? 0));

        // Calculate tax amount
        invoice.TaxAmount = invoice.TaxRate.HasValue ? (int)(invoice.Subtotal * (invoice.TaxRate / 100.0)) : 0;

        // Calculate discount amount
        invoice.DiscountAmount = invoice.Discount ?? 0;

        // Calculate total
        invoice.Total = invoice.Subtotal + invoice.TaxAmount - invoice.DiscountAmount;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context?.Dispose();
            }

            _disposed = true;
        }
    }
}