
using hoistmt.Models;
using Microsoft.EntityFrameworkCore;



public class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public DbSet<Vehicle> vehicles { get; set; }
    public DbSet<Appointment> appointments { get; set; }
    public DbSet<Account> accounts { get; set; }
    public DbSet<Customer> customers { get; set; }
    public DbSet<invoiceEntry> invoiceEntries { get; set; }
    public DbSet<Invoice> invoices { get; set; }
    public DbSet<eventAttribute> eventAttributes { get; set; }

    
}