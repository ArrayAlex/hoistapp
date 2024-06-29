
using hoistmt.Models;
using hoistmt.Models.MasterDbModels;
using hoistmt.Models.Tenant.Billing;
using Microsoft.EntityFrameworkCore;
using Vehicle = hoistmt.Models.Vehicle;


public class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public DbSet<Vehicle> vehicles { get; set; }
    public DbSet<Appointment> appointments { get; set; }
    public DbSet<CompanInvoice> companyinvoices { get; set; }
    public DbSet<UserAccount> accounts { get; set; }
    public DbSet<PaymentGateway> paymentgateway { get; set; }
    
    public DbSet<TenantTransactions> tenanttransactions { get; set; }
    public DbSet<Customer> customers { get; set; }
    public DbSet<invoiceEntry> invoiceEntries { get; set; }
    public DbSet<Invoice> invoices { get; set; }
    public DbSet<eventAttribute> eventAttributes { get; set; }

    
}