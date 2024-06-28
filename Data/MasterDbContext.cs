using hoistmt.HttpClients;
using hoistmt.Models;
using hoistmt.Models.Account;
using hoistmt.Models.MasterDbModels;
using VehicleData = hoistmt.Models.MasterDbModels.Vehicle;
using Vehicle = hoistmt.Models.Vehicle;

namespace hoistmt.Data;
using Microsoft.EntityFrameworkCore;


public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Session> sessions { get; set; }
    public DbSet<VehicleData> vehicledata { get; set; }
    
    public DbSet<Companies> Companies { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<UserAccount> Accounts { get; set; }

}