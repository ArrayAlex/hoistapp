using hoistmt.Exceptions;
using hoistmt.Interfaces;
using hoistmt.Models;
using Microsoft.EntityFrameworkCore;
using Invoice = hoistmt.Models.Invoice;

namespace hoistmt.Services.lib;

public class LibraryInvoiceService : IDisposable
{
    private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
    private TenantDbContext _context;
    private bool _disposed = false;

    public LibraryInvoiceService(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
    {
        _tenantDbContextResolver =
            tenantDbContextResolver ?? throw new ArgumentNullException(nameof(tenantDbContextResolver));
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
        var invoices = await _context.invoices.ToListAsync();
        if (!invoices.Any())
        {
            throw new NotFoundException("No invoices found.");
        }

        return invoices;
    }

    public async Task<Invoice> GetInvoice(int invoiceId)
    {
        await EnsureContextInitializedAsync();
        var invoice = await _context.invoices.FirstOrDefaultAsync(i => i.invoice_id == invoiceId);
        //.Include(i => i.Entries) // Include related entries

        if (invoice == null)
        {
            throw new NotFoundException($"Invoice with ID {invoiceId} not found.");
        }

        return invoice;
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByVehicleId(int vehicleId)
    {
        await EnsureContextInitializedAsync();
        var invoices = await _context.invoices.Where(i => i.vehicle_id == vehicleId).ToListAsync();
        if (!invoices.Any())
        {
            throw new NotFoundException($"No invoices found for vehicle with ID {vehicleId}.");
        }

        return invoices;
    }

    public async Task<Invoice> UpdateInvoice(Invoice invoice)
    {
        await EnsureContextInitializedAsync();

        var invoiceEntity = await _context.invoices.FindAsync(invoice.invoice_id);
        if (invoiceEntity == null)
        {
            throw new NotFoundException($"Invoice with ID {invoice.invoice_id} not found.");
        }

        invoiceEntity.total_amount = invoice.total_amount;
        invoiceEntity.payment_status = invoice.payment_status;
        invoiceEntity.customerid = invoice.customerid;
        await _context.SaveChangesAsync();
        return invoiceEntity;
    }

    public async Task<Invoice> AddInvoice(Invoice invoice)
    {
        await EnsureContextInitializedAsync();
        var invoiceEntity = new Invoice
        {
            customerid = invoice.invoice_id,
            invoice_date = DateOnly.FromDateTime(DateTime.Now),
            total_amount = invoice.total_amount,
            payment_status = invoice.payment_status,
            created_at = DateTime.Now,
            updated_at = DateTime.Now
        };
        
        _context.invoices.Add(invoiceEntity);

        // Save changes to the database
        await _context.SaveChangesAsync();

        return invoiceEntity;

    }
    
    public async Task<Invoice> DeleteInvoice(int invoiceId)
    {
        await EnsureContextInitializedAsync();
        var invoice = await _context.invoices.FindAsync(invoiceId);
        if (invoice == null)
        {
            throw new NotFoundException($"Invoice with ID {invoiceId} not found.");
        }

        _context.invoices.Remove(invoice);
        await _context.SaveChangesAsync();
        return invoice;
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