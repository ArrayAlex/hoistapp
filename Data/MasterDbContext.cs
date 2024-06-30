﻿using hoistmt.Functions;
using hoistmt.Models;

using hoistmt.Models.MasterDbModels;
using VehicleData = hoistmt.Models.MasterDbModels.Vehicle;
using Vehicle = hoistmt.Models.Vehicle;

namespace hoistmt.Data;
using Microsoft.EntityFrameworkCore;


public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    public DbSet<DbTenant> tenants { get; set; }
    public DbSet<Session> sessions { get; set; }
    public DbSet<CompanInvoice> companyinvoices { get; set; }
    public DbSet<VehicleData> vehicledata { get; set; }
    public DbSet<TenantTransactions> tenanttransactions { get; set; }
    public DbSet<Subscription> plansubscriptions { get; set; }
    public DbSet<Companies> Companies { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<UserAccount> Accounts { get; set; }

}