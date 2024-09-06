using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using hoistmt.Models;


namespace hoistmt.Services.lib
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    public class NotFound : Exception
    {
        public NotFound(string message) : base(message) { }
    }
    
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetVehiclesAsync();
    }

    public class VehicleService : IVehicleService, IDisposable
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private TenantDbContext _context;
        private bool _disposed = false;
        
        public VehicleService(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
        {
            _tenantDbContextResolver = tenantDbContextResolver ?? throw new ArgumentNullException(nameof(tenantDbContextResolver));
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
        
        public async Task<IEnumerable<Vehicle>> GetVehiclesAsync()
        {
            await EnsureContextInitializedAsync();
            return await _context.vehicles.ToListAsync();
        }
        
        public async Task<Vehicle> AddVehicleAsync(Vehicle vehicle)
        {
            await EnsureContextInitializedAsync();
            _context.vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle> GetVehicleDetails(int id)
        {
            await EnsureContextInitializedAsync();
            return await _context.vehicles.FindAsync(id);
        }
        
        public async Task<IEnumerable<Vehicle>> GetVehiclesByCustomerId(int customerId)
        {
            await EnsureContextInitializedAsync();
            return await _context.vehicles.Where(v => v.customerid == customerId).ToListAsync();
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
}