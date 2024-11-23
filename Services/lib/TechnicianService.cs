using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using hoistmt.Interfaces;
using Microsoft.EntityFrameworkCore;
using hoistmt.Models;
using hoistmt.Models.Tenant;
using SendGrid.Helpers.Errors.Model;

namespace hoistmt.Services.lib
{
    public class TechnicianService
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private TenantDbContext _context;

        public TechnicianService(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
        {
            _tenantDbContextResolver = tenantDbContextResolver ??
                                       throw new ArgumentNullException(nameof(tenantDbContextResolver));
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




        
        // public async Task<Technician> UpdateTechnicianAsync(int TechnicianboardId, int TechnicianId, Technician updatedTechnician)
        // {
        //     await EnsureContextInitializedAsync();
        //
        //     // Find the Technician by TechnicianBoardId and TechnicianId
        //     var Technician = await _context.Technicians
        //         .FirstOrDefaultAsync(j => j.TechnicianId == TechnicianId && j.TechnicianBoardID == TechnicianboardId);
        //
        //     if (Technician == null)
        //     {
        //         return null; // Return null if the Technician doesn't exist
        //     }
        //
        //     // No updates performed yet, just returning the Technician
        //     return Technician; 
        // }
        //





        // public async Task<IEnumerable<Technician>> GetTechniciansByAppointmentId(int appointmentId)
        // {
        //     await EnsureContextInitializedAsync();
        //
        //     
        // }

        public async Task<object> GetTechnicianDetails(int id)
        {
            await EnsureContextInitializedAsync();

            // Use FirstOrDefaultAsync to get a single technician by ID
            return await _context.accounts
                .Where(t => t.isTech == true && t.Id == id)  // Assuming 'Id' is the primary key or identifier
                .FirstOrDefaultAsync();
        }

        // public async Task<IEnumerable<Technician>> GetTechniciansByCustomerId(int customerId)
        // {
        //     await EnsureContextInitializedAsync();
        //     return await _context.accounts.Where(j => j.CustomerId == customerId).ToListAsync();
        // }

        public async Task DeleteTechnician(int TechnicianId)
        {
            await EnsureContextInitializedAsync();
            var Technician = await _context.accounts.FindAsync(TechnicianId);
            if (Technician == null)
            {
                throw new NotFoundException("Technician not found.");
            }

            _context.accounts.Remove(Technician);
            await _context.SaveChangesAsync();
        }

        // public async Task<IEnumerable<TechnicianWithDetails>> SearchTechnicians(string searchTerm)
        // {
        //     await EnsureContextInitializedAsync();
        //
        //     if (string.IsNullOrWhiteSpace(searchTerm))
        //         return await GetTechniciansAsync();
        //
        //     return await _context.Technicians
        //         .Where(j => j.Notes.ToLower().Contains(searchTerm.ToLower()) ||
        //                     j.TechnicianId.ToString().Contains(searchTerm))
        //         .ToListAsync();
        // }
        
        public async Task<List<UserAccount>> GetTechniciansAsync()
        {
            await EnsureContextInitializedAsync();
    
            // Filter technicians based on the isTech column (assuming 1 means they are a technician)
            return await _context.accounts
                .Where(t => t.isTech == true) // Only technicians
                .ToListAsync();
        }

        // public async Task<Technician> UpdateTechnician(Technician Technician)
        // {
        //     await EnsureContextInitializedAsync();
        //     var existingTechnician = await _context.Technicians.FindAsync(Technician.TechnicianId);
        //     if (existingTechnician == null)
        //     {
        //         throw new NotFoundException("Technician not found");
        //     }
        //
        //     // Update the properties of the existing Technician
        //     existingTechnician.CustomerId = Technician.CustomerId;
        //     existingTechnician.VehicleId = Technician.VehicleId;
        //     existingTechnician.TechnicianId = Technician.TechnicianId;
        //
        //
        //     existingTechnician.Notes = Technician.Notes;
        //
        //     existingTechnician.UpdatedAt = DateTime.UtcNow;
        //
        //     await _context.SaveChangesAsync();
        //     return existingTechnician;
        // }





 
        
    }
}