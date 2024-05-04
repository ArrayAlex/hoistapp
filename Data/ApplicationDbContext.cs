using hoistmt.Models;

namespace hoistmt.Data;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Session> sessions { get; set; }
    
    public DbSet<Vehicle> vehicles { get; set; }
    public DbSet<Appointment> appointments { get; set; }
    public DbSet<Account> Accounts { get; set; }
    
}