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

    // public async Task<InvoiceResponse> GetInvoiceAsync(int id)
    // {
    //     var invoice = await _context.invoices
    //         .Include(i => i.Jobs)
    //         .Include(i => i.AdHocEntries)
    //         .Where(i => i.invoice_id == id)
    //         .Select(invoice => new InvoiceResponse
    //         {
    //             invoice_id = invoice.invoice_id,
    //             customerid = invoice.customerid,
    //             invoice_date = invoice.invoice_date,
    //             total_amount = invoice.total_amount,
    //             payment_status = invoice.payment_status,
    //             created_at = invoice.created_at,
    //             updated_at = invoice.updated_at,
    //             items = (invoice.Jobs.Select(job => new InvoiceItemResponse
    //                 {
    //                     type = "job",
    //                     item_id = job.job_id,
    //                     description = job.notes,
    //                     amount = job.amount
    //                 }) ?? Enumerable.Empty<InvoiceItemResponse>())
    //                 .Concat(invoice.AdHocEntries.Select(adhoc => new InvoiceItemResponse
    //                 {
    //                     type = "adhoc",
    //                     item_id = adhoc.adhoc_id,
    //                     description = adhoc.description,
    //                     amount = adhoc.amount
    //                 }) ?? Enumerable.Empty<InvoiceItemResponse>())
    //                 .ToList()
    //         })
    //         .FirstOrDefaultAsync();
    //
    //     if (invoice == null)
    //     {
    //         throw new NotFoundException($"Invoice with ID {id} not found.");
    //     }
    //
    //     return invoice;
    // }

    // public async Task<int> CreateInvoice(InvoiceDTO invoiceData)
    // {
    //     await EnsureContextInitializedAsync();
    //     if (invoiceData == null)
    //         throw new ArgumentNullException(nameof(invoiceData));
    //
    //     if (invoiceData.Items == null || !invoiceData.Items.Any())
    //         throw new ArgumentException("Invoice must contain at least one item");
    //
    //     if (_context == null)
    //         throw new InvalidOperationException("Database context is not initialized");
    //
    //     using var transaction = await _context.Database.BeginTransactionAsync();
    //
    //     try
    //     {
    //         TimeZoneInfo nzTimeZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
    //         DateTime nzTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, nzTimeZone);
    //         // Create the main invoice record
    //         var invoice = new Invoice
    //         {
    //             created_at = nzTime,
    //             updated_at = nzTime,
    //             invoice_date = invoiceData.DueDate,
    //             customerid = invoiceData.customerid,
    //             total_amount = invoiceData.TotalAmount,
    //         };
    //
    //         _context.invoices.Add(invoice);
    //         await _context.SaveChangesAsync();
    //
    //         // Process each invoice item
    //         foreach (var item in invoiceData.Items)
    //         {
    //             if (item == null)
    //                 continue; // Skip null items or throw exception based on your requirements
    //
    //             var invoiceItem = new InvoiceItem
    //             {
    //                 invoice_id = invoice.invoice_id,
    //                 Amount = item.Amount,
    //                 ItemType = item.Type
    //             };
    //
    //             if (item.Type == "job" && item.JobId > 0)
    //             {
    //                 // Verify the job exists and update its status
    //                 var job = await _context.jobs.FindAsync(item.JobId);
    //
    //                 if (job == null)
    //                     throw new Exception($"Job with ID  not found");
    //             }
    //
    //             _context.invoiceitems.Add(invoiceItem);
    //         }
    //
    //         await _context.SaveChangesAsync();
    //         await transaction.CommitAsync();
    //
    //         return invoice.invoice_id;
    //     }
    //     catch (Exception)
    //     {
    //         await transaction.RollbackAsync();
    //         throw;
    //     }
    // }

    public async Task<object> GetInvoicesAsync()
    {
        await EnsureContextInitializedAsync();
        var customers = await _context.customers.ToListAsync();
        // Step 1: Get all invoices
        var invoices = await _context.invoices
            .Select(invoice => new
            {
                invoice.invoice_id,
                invoice.customerid,
                invoice.invoice_date,
                invoice.total_amount,
                customer = _context.customers
                    .Where(c => c.id == invoice.customerid)
                    .Select(c => new
                    {
                        c.FirstName,
                        c.LastName // Example if you want additional attributes
                    })
                    .FirstOrDefault(), // Fetch the first matching customer or null if none exists
                jobs = new List<object>(),  // Replace with actual job data logic
                adhoc = new List<object>()  // Replace with actual adhoc data logic
            })
            .ToListAsync();

        // Step 2: Get all jobs, adhoc entries, and jobtypes
        var jobs = await _context.jobs.ToListAsync();
        var adhoc = await _context.adhocentries.ToListAsync();
        var jobTypes = await _context.jobtypes.ToListAsync(); // Assuming this table contains jobtypeid and hourly_rate

        foreach (var invoice in invoices)
        {
            // Find the jobs that match the current invoice's invoice_id
            var matchingJobs = jobs.Where(job => job.invoice_id == invoice.invoice_id)
                .Select(job => new
                {
                    job.JobId,
                    job.Notes,
                    job.hours_worked,
                    amount = job.hours_worked > 0
                        ? jobTypes
                            .Where(jt => jt.id == job.JobTypeID)
                            .Select(jt =>
                                jt.hourly_rate *
                                job.hours_worked) // Calculate amount based on hourly_rate * hours_worked
                            .FirstOrDefault() // Assuming only one match will exist for each jobtypeid
                        : 0 // Set amount to 0 if hours_worked is 0 or less
                })
                .ToList();

            // Add the jobs to the invoice's jobs list
            invoice.jobs.AddRange(matchingJobs);

            // Find the adhoc entries that match the current invoice's invoice_id
            var matchingAdhoc = adhoc.Where(a => a.invoice_id == invoice.invoice_id)
                .Select(a => new
                {
                    a.invoice_id,
                    a.Description,
                    a.Amount
                })
                .ToList();

            // Add the adhoc entries to the invoice's adhoc list
            invoice.adhoc.AddRange(matchingAdhoc);
        }

        // Return the list of invoices with their corresponding jobs and adhoc entries
        return invoices;
    }

    // public async Task<List<InvoiceResponse>> GetInvoicesAsync()
    // {
    //     var invoices = await _context.invoices
    //         .Include(i => i.Jobs)
    //         .Include(i => i.AdHocEntries)
    //         .Select(invoice => new InvoiceResponse
    //         {
    //             invoice_id = invoice.invoice_id,
    //             customerid = invoice.customerid,
    //             invoice_date = invoice.invoice_date,
    //             total_amount = invoice.total_amount,
    //             payment_status = invoice.payment_status,
    //             created_at = invoice.created_at,
    //             updated_at = invoice.updated_at,
    //             items = invoice.Jobs
    //                 .Select(job => new InvoiceItemResponse
    //                 {
    //                     type = "job",
    //                     item_id = job.job_id,
    //                     description = job.notes,
    //                     amount = job.amount
    //                 })
    //                 .Concat(invoice.AdHocEntries
    //                     .Select(adhoc => new InvoiceItemResponse
    //                     {
    //                         type = "adhoc",
    //                         item_id = adhoc.adhoc_id,
    //                         description = adhoc.description,
    //                         amount = adhoc.amount
    //                     }))
    //                 .ToList()
    //         })
    //         .ToListAsync();
    //
    //     if (!invoices.Any())
    //     {
    //         throw new NotFoundException("No invoices found.");
    //     }
    //
    //     return invoices;
    // }    
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

    // public async Task<IEnumerable<Invoice>> GetInvoicesByVehicleId(int vehicleId)
    // {
    //     await EnsureContextInitializedAsync();
    //     var invoices = await _context.invoices.Where(i => i.vehicle_id == vehicleId).ToListAsync();
    //     if (!invoices.Any())
    //     {
    //         throw new NotFoundException($"No invoices found for vehicle with ID {vehicleId}.");
    //     }
    //
    //     return invoices;
    // }


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