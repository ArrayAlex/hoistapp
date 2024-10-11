using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using hoistmt.Interfaces;
using Microsoft.EntityFrameworkCore;
using hoistmt.Models;
using SendGrid.Helpers.Errors.Model;

namespace hoistmt.Services.lib
{
    public class JobService
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private TenantDbContext _context;

        public JobService(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
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
        
        public async Task<IEnumerable<Job>> GetJobsAsync()
        {
            await EnsureContextInitializedAsync();
            return await _context.jobs.ToListAsync();
        }
        
        public async Task<Job> AddJobAsync(Job job)
        {
            await EnsureContextInitializedAsync();
            _context.jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<Job> GetJobDetails(int id)
        {
            await EnsureContextInitializedAsync();
            return await _context.jobs.FindAsync(id);
        }
        
        public async Task<IEnumerable<Job>> GetJobsByCustomerId(int customerId)
        {
            await EnsureContextInitializedAsync();
            return await _context.jobs.Where(j => j.CustomerId == customerId).ToListAsync();
        }
        
        public async Task DeleteJob(int jobId)
        {
            await EnsureContextInitializedAsync();
            var job = await _context.jobs.FindAsync(jobId);
            if (job == null)
            {
                throw new NotFoundException("Job not found.");
            }
            _context.jobs.Remove(job);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Job>> SearchJobs(string searchTerm)
        {
            await EnsureContextInitializedAsync();

            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetJobsAsync();

            return await _context.jobs
                .Where(j => j.Description.ToLower().Contains(searchTerm.ToLower()) ||
                            j.Notes.ToLower().Contains(searchTerm.ToLower()) ||
                            j.Status.ToLower().Contains(searchTerm.ToLower()) ||
                            j.JobId.ToString().Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<Job> UpdateJob(Job job)
        {
            await EnsureContextInitializedAsync();
            var existingJob = await _context.jobs.FindAsync(job.JobId);
            if (existingJob == null)
            {
                throw new NotFoundException("Job not found");
            }
            
            // Update the properties of the existing job
            existingJob.CustomerId = job.CustomerId;
            existingJob.VehicleId = job.VehicleId;
            existingJob.TechnicianId = job.TechnicianId;
            existingJob.InvoiceId = job.InvoiceId;
            existingJob.StartDate = job.StartDate;
            existingJob.EndDate = job.EndDate;
            existingJob.PickupDate = job.PickupDate;
            existingJob.Description = job.Description;
            existingJob.Notes = job.Notes;
            existingJob.Status = job.Status;
            existingJob.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingJob;
        }
    }
}