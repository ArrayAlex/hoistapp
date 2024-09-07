using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using hoistmt.Interfaces;
using Microsoft.EntityFrameworkCore;
using hoistmt.Models;
using SendGrid.Helpers.Errors.Model;

namespace hoistmt.Services.lib
{
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
        
        public async Task<Vehicle> DeleteVehicle(int vehicleId)
        {
            await EnsureContextInitializedAsync();
            var vehicle = await _context.vehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                throw new NotFoundException("Vehicle not found.");
            }
            _context.vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle> UpdateVehicle(Vehicle vehicle)
        {
            await EnsureContextInitializedAsync();
            var existingVehicle = await _context.vehicles.FindAsync(vehicle.id);
            if (existingVehicle == null)
            {
                throw new NotFoundException("Vehicle not found"); // Vehicle with the specified ID not found
            }
            existingVehicle.make = vehicle.make;
            existingVehicle.model = vehicle.model;
            existingVehicle.year = vehicle.year;
            existingVehicle.customerid = vehicle.customerid;
            existingVehicle.description = vehicle.description;
            
            /*if (vehicle.customerid != null)
            {
                // Find the corresponding customer
                var existingCustomer = await dbContext.customers.FindAsync(vehicle.customerid);
                if (existingCustomer == null)
                {
                    return NotFound("Customer with the specified ID not found");
                }

                // Update the owner field of the vehicle with customer's first name and last name
                existingVehicle.owner = existingCustomer.FirstName + " " + existingCustomer.LastName;
            }*/
            await _context.SaveChangesAsync();
            return vehicle;
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