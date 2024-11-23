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
    public class JobService
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private TenantDbContext _context;

        public JobService(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
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

        public async Task<IEnumerable<JobWithDetails>> GetJobsAsync()
        {
            await EnsureContextInitializedAsync();

            var query = from job in _context.jobs
                join jobStatus in _context.jobstatus on job.JobStatusID equals jobStatus.id
                join jobType in _context.jobtypes on job.JobTypeID equals jobType.id
                
                join vehicle in _context.vehicles on job.VehicleId equals vehicle.id into vehicleJoin
                from vehicle in vehicleJoin.DefaultIfEmpty() 
                
                join customer in _context.customers on job.CustomerId equals customer.id into customerJoin
                from customer in customerJoin.DefaultIfEmpty() 
                
                join account in _context.accounts on job.TechnicianId equals account.Id into accountJoin
                from account in accountJoin.DefaultIfEmpty() 
                
                select new JobWithDetails
                {
                    JobId = job.JobId,
                    CustomerId = job.CustomerId,
                    VehicleId = job.VehicleId,
                    TechnicianId = job.TechnicianId,
                    Notes = job.Notes,
                    UpdatedAt = job.UpdatedAt,
                    CreatedAt = job.CreatedAt,
                    AppointmentId = job.AppointmentId,
                    JobBoardID = job.JobBoardID,
                    JobStatus = new JobStatusDetails
                    {
                        Id = job.JobStatusID,
                        Title = jobStatus.title,
                        Color = jobStatus.color
                    },
                    JobType = new JobTypeDetails
                    {
                        Id = job.JobTypeID,
                        Title = jobType.title,
                        Color = jobType.color
                    },
                    // If no matching customer, the properties will be null
                    Customer = customer == null
                        ? null
                        : new Customer
                        {
                            id = customer.id,
                            FirstName = customer.FirstName,
                            LastName = customer.LastName,
                            Email = customer.Email,
                            Phone = customer.Phone,
                            DOB = customer.DOB,
                            created_at = customer.created_at,
                            postal_address = customer.postal_address,
                            notes = customer.notes,
                            modified_at = customer.modified_at
                        },
                    // If no matching vehicle, the properties will be null
                    Vehicle = vehicle == null
                        ? null
                        : new Vehicle
                        {
                            id = vehicle.id,
                            customerid = vehicle.customerid,
                            owner = vehicle.owner,
                            make = vehicle.make,
                            description = vehicle.description,
                            model = vehicle.model,
                            rego = vehicle.rego,
                            vin = vehicle.vin,
                            year = vehicle.year
                        },
                    Technician = account == null
                        ? null
                        : new Technician()
                        {
                            Id = account.Id,
                            Name = account.Name
 /*       public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string? contact { get; set; }
        public string email { get; set; }
        public bool Active { get; set; }
        public string Username { get; set; }

        public string? roleName { get; set; }

        public string? position { get; set; }

        public string? phone { get; set; }

        public int roleID { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public string? VerificationToken { get; set; }
        public DateTime? VerificationTokenExpiry { get; set; }
        public bool? IsVerified { get; set; }

        public bool? isTech { get; set; }*/

                        }
                    
                };

            return await query.ToListAsync();
        }

        public async Task<Job> AddJobAsync(Job job)
        {
            await EnsureContextInitializedAsync();
            _context.jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }
        
        public async Task<Job> UpdateJobAsync(int jobboardId, int jobId, Job updatedJob)
        {
            await EnsureContextInitializedAsync();

            // Find the job by JobBoardId and JobId
            var job = await _context.jobs
                .FirstOrDefaultAsync(j => j.JobId == jobId && j.JobBoardID == jobboardId);

            if (job == null)
            {
                return null; // Return null if the job doesn't exist
            }

            // No updates performed yet, just returning the job
            return job; 
        }
        
        public async Task<Job> UpdateJobBoardIdAsync(int jobId, int newJobBoardId)
        {
            await EnsureContextInitializedAsync();

            // Find the job by JobId
            var job = await _context.jobs
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null)
            {
                return null; // Return null if the job doesn't exist
            }

            // Update the jobBoardID
            job.JobBoardID = newJobBoardId;

            // Save changes to the database
            _context.jobs.Update(job);
            await _context.SaveChangesAsync();

            return job; // Return the updated job
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
                        Title = _context.jobstatus.Where(js => js.id == j.JobStatusID).Select(js => js.title)
                            .FirstOrDefault(),
                        Color = _context.jobstatus.Where(js => js.id == j.JobStatusID).Select(js => js.color)
                            .FirstOrDefault()
                    },
                    JobType = new JobTypeDetails
                    {
                        Id = j.JobTypeID,
                        Title = _context.jobtypes.Where(jt => jt.id == j.JobTypeID).Select(jt => jt.title)
                            .FirstOrDefault(),
                        Color = _context.jobtypes.Where(jt => jt.id == j.JobTypeID).Select(jt => jt.color)
                            .FirstOrDefault()
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

        // public async Task<IEnumerable<JobWithDetails>> SearchJobs(string searchTerm)
        // {
        //     await EnsureContextInitializedAsync();
        //
        //     if (string.IsNullOrWhiteSpace(searchTerm))
        //         return await GetJobsAsync();
        //
        //     return await _context.jobs
        //         .Where(j => j.Notes.ToLower().Contains(searchTerm.ToLower()) ||
        //                     j.JobId.ToString().Contains(searchTerm))
        //         .ToListAsync();
        // }

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