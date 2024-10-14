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

        public async Task<IEnumerable<JobWithDetails>> GetJobsByAppointmentId(int appointmentId)
        {
            await EnsureContextInitializedAsync();
            return await _context.jobs
                .Where(j => j.AppointmentId == appointmentId)
                .Select(j => new JobWithDetails
                {
                    JobId = j.JobId,
                    CustomerId = j.CustomerId,
                    VehicleId = j.VehicleId,
                    TechnicianId = j.TechnicianId,
                    Notes = j.Notes,
                    UpdatedAt = j.UpdatedAt,
                    CreatedAt = j.CreatedAt,
                    AppointmentId = j.AppointmentId,
                    JobStatus = new JobStatusDetails
                    {
                        Id = j.JobStatusID,
                        Title = _context.jobstatus.Where(js => js.id == j.JobStatusID).Select(js => js.title).FirstOrDefault(),
                        Color = _context.jobstatus.Where(js => js.id == j.JobStatusID).Select(js => js.color).FirstOrDefault()
                    },
                    JobType = new JobTypeDetails
                    {
                        Id = j.JobTypeID,
                        Title = _context.jobtypes.Where(jt => jt.id == j.JobTypeID).Select(jt => jt.title).FirstOrDefault(),
                        Color = _context.jobtypes.Where(jt => jt.id == j.JobTypeID).Select(jt => jt.color).FirstOrDefault()
                    }
                })
                .ToListAsync();
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
                .Where(j => j.Notes.ToLower().Contains(searchTerm.ToLower()) ||
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



            existingJob.Notes = job.Notes;

            existingJob.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingJob;
        }

        // public async Task<Job> UpdateJobStatus(int jobId, int statusId)
        // {
        //     await EnsureContextInitializedAsync();
        //     var job = await _context.jobs.FindAsync(jobId);
        //     if (job == null)
        //     {
        //         throw new NotFoundException("Job not found");
        //     }
        //     var status = await _context.jobstatus.FindAsync(statusId);
        //     if (status == null)
        //     {
        //         throw new NotFoundException("Job status not found");
        //     }
        //     job. = status.title;
        //     job.UpdatedAt = DateTime.UtcNow;
        //     await _context.SaveChangesAsync();
        //     return job;
        // }

        public async Task<JobStatus> AddJobStatus(JobStatus jobStatus)
        {
            await EnsureContextInitializedAsync();
            _context.jobstatus.Add(jobStatus);
            await _context.SaveChangesAsync();
            return jobStatus;
        }

        public async Task DeleteJobStatus(int statusId)
        {
            await EnsureContextInitializedAsync();
            var status = await _context.jobstatus.FindAsync(statusId);
            if (status == null)
            {
                throw new NotFoundException("Job status not found");
            }
            _context.jobstatus.Remove(status);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<JobStatus>> GetJobStatuses()
        {
            await EnsureContextInitializedAsync();
            return await _context.jobstatus.ToListAsync();
        }

        public async Task<JobStatus> GetJobStatusByID(int statusId)
        {
            await EnsureContextInitializedAsync();
            var status = await _context.jobstatus.FindAsync(statusId);
            if (status == null)
            {
                throw new NotFoundException("Job status not found");
            }
            return status;
        }

        public async Task<IEnumerable<JobTypes>> GetJobTypes()
        {
            await EnsureContextInitializedAsync();
            return await _context.jobtypes.ToListAsync();
        }

        public async Task<JobTypes> GetJobTypeByID(int typeId)
        {
            await EnsureContextInitializedAsync();
            var jobType = await _context.jobtypes.FindAsync(typeId);
            if (jobType == null)
            {
                throw new NotFoundException("Job type not found");
            }
            return jobType;
        }

        public async Task<JobTypes> UpdateJobType(JobTypes jobType)
        {
            await EnsureContextInitializedAsync();
            var existingJobType = await _context.jobtypes.FindAsync(jobType.id);
            if (existingJobType == null)
            {
                throw new NotFoundException("Job type not found");
            }
            existingJobType.title = jobType.title;
            existingJobType.color = jobType.color;
            await _context.SaveChangesAsync();
            return existingJobType;
        }

        public async Task<JobTypes> AddJobType(JobTypes jobType)
        {
            await EnsureContextInitializedAsync();
            _context.jobtypes.Add(jobType);
            await _context.SaveChangesAsync();
            return jobType;
        }

        public async Task DeleteJobType(int typeId)
        {
            await EnsureContextInitializedAsync();
            var jobType = await _context.jobtypes.FindAsync(typeId);
            if (jobType == null)
            {
                throw new NotFoundException("Job type not found");
            }
            _context.jobtypes.Remove(jobType);
            await _context.SaveChangesAsync();
        }

        
    }
}