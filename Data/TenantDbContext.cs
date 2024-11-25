
using hoistmt.Models;
using hoistmt.Models.Billing;
using hoistmt.Models.MasterDbModels;
using hoistmt.Models.Tenant;
using hoistmt.Models.Tenant.Billing;
using Microsoft.EntityFrameworkCore;
using Vehicle = hoistmt.Models.Vehicle;


public class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public DbSet<Vehicle> vehicles { get; set; }
    public DbSet<Job> jobs { get; set; }
    public DbSet<InvoiceItem> invoiceitems { get; set; }
    public DbSet<JobStatus> jobstatus { get; set; }

    public DbSet<BookingStatus> bookingstatus { get; set; }
    public DbSet<JobTypes> jobtypes { get; set; }
    public DbSet<Appointment> appointments { get; set; }
    //public DbSet<CompanInvoice> companyinvoices { get; set; }
    public DbSet<UserAccount> accounts { get; set; }
    public DbSet<AccountBillingInfo> company { get; set; }
    public DbSet<PaymentGateway> paymentgateway { get; set; }
    public DbSet<Booking> Bookings { get; set; } // Ensure this matches the reference in the controller

    
    public DbSet<TenantTransactions> tenanttransactions { get; set; }
    public DbSet<Customer> customers { get; set; }
    public DbSet<invoiceEntry> invoiceEntries { get; set; }
    public DbSet<LineItem> LineItems { get; set; }
    public DbSet<Invoice> invoices { get; set; }
    public DbSet<AdhocEntry> adhocentries { get; set; }
    public DbSet<eventAttribute> eventAttributes { get; set; }
    
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     base.OnModelCreating(modelBuilder);
    //
    //     modelBuilder.Entity<Invoice>(entity =>
    //     {
    //         entity.ToTable("invoices");
    //         entity.HasKey(e => e.invoice_id);
    //         
    //         entity.HasMany(e => e.LineItems)
    //             .WithOne(e => e.Invoice)
    //             .HasForeignKey(e => e.invoice_id)
    //             .OnDelete(DeleteBehavior.Cascade);
    //     });
    //
    //     modelBuilder.Entity<LineItem>(entity =>
    //     {
    //         entity.ToTable("lineitems");
    //         entity.HasKey(e => e.Id);
    //         
    //         entity.HasOne(e => e.Invoice)
    //             .WithMany(e => e.LineItems)
    //             .HasForeignKey(e => e.invoice_id)
    //             .OnDelete(DeleteBehavior.Cascade);
    //     });
    // }

    
}