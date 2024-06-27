using hoistmt.Models;
using hoistmt.Models.MasterDbModels;

namespace hoistmt.Data;
using Microsoft.EntityFrameworkCore;

public class MasterDbContext : DbContext
{
    
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
    {
    }
    
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Session> sessions { get; set; }
    public DbSet<Companies> Companies { get; set; }
    
    public DbSet<Vehicle> vehicles { get; set; }
    public DbSet<Appointment> appointments { get; set; }
    public DbSet<Account> Accounts { get; set; }
    
}