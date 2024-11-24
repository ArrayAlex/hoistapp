using hoistmt.Models;
using hoistmt.Services;
using hoistmt.Services.lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hoistmt.Exceptions;
using hoistmt.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private readonly JobService _jobService;

        public JobController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, JobService jobService)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _jobService = jobService;
        }
        
        [HttpGet("jobs")]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            try
            {
                var jobs = await _jobService.GetJobsAsync();
                return Ok(jobs);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        
        // [HttpGet("job")]
        // public async Task<ActionResult<IEnumerable<Job>>> GetJob([FromQuery] int appointmentId)
        // {
        //     try
        //     {
        //         var jobs = await _jobService.GetJobsByAppointmentId(appointmentId);
        //         return Ok(jobs);
        //     }
        //     catch (UnauthorizedException ex)
        //     {
        //         return Unauthorized(ex.Message);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, "An error occurred while processing your request.");
        //     }
        // }

        [HttpPost("add")]
        public async Task<IActionResult> AddJob([FromBody] NewJob newJob)
        {
            try
            {
                var addedJob = await _jobService.AddJobAsync(newJob); // Passing NewJob
                return CreatedAtAction(nameof(GetJobDetails), new { id = addedJob.JobId }, addedJob);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        
        [HttpPut("job/{id}")]
        public async Task<IActionResult> GetJobDetails(int id)
        {
            try
            {
                var job = await _jobService.GetJobDetails(id);
                if (job == null)
                {
                    return NotFound($"Job with ID {id} not found.");
                }
                return Ok(job);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        
        [HttpPut("jobboardid/{jobId}")]
        public async Task<IActionResult> UpdateJobDetails(int jobId, [FromBody] int newJobBoardId)
        {
            try
            {
                var job = await _jobService.UpdateJobBoardIdAsync(jobId, newJobBoardId);
                if (job == null)
                {
                    return NotFound($"Job with ID {jobId} not found.");
                }
                return Ok(job);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Customer>>> SearchJobs([FromQuery] string searchTerm)
        {
            try
            {
                var jobs = await _jobService.SearchJobs(searchTerm);
                return Ok(jobs);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Error searching Jobs: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
       

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetJobsByCustomerId(int customerId)
        {
            try
            {
                var jobs = await _jobService.GetJobsByCustomerId(customerId);
                if (!jobs.Any())
                {
                    return NotFound($"No jobs found for customer ID {customerId}");
                }
                return Ok(jobs);
            }
            catch(UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        
        [HttpDelete("delete/{jobId}")]
        public async Task<IActionResult> DeleteJob(int jobId)
        {
            try
            {
                await _jobService.DeleteJob(jobId);
                return Ok("Job deleted successfully");
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateJob([FromBody] Job job)
        {
            try
            {
                var updatedJob = await _jobService.UpdateJob(job);
                return Ok(updatedJob);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        // [HttpPut("updateStatus/{jobId}")]
        // public async Task<IActionResult> UpdateJobStatus(int jobId, [FromBody] int statusId)
        // {
        //     try
        //     {
        //         var updatedJob = await _jobService.UpdateJobStatus(jobId, statusId);
        //         return Ok(updatedJob);
        //     }
        //     catch (UnauthorizedException ex)
        //     {
        //         return Unauthorized(ex.Message);
        //     }
        //     catch (NotFoundException ex)
        //     {
        //         return NotFound(ex.Message);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, "An error occurred while processing your request.");
        //     }
        // }

        [HttpPost("status")]
        public async Task<IActionResult> AddJobStatus([FromBody] JobStatus jobStatus)
        {
            try
            {
                var addedStatus = await _jobService.AddJobStatus(jobStatus);
                return CreatedAtAction(nameof(GetJobStatusByID), new { statusId = addedStatus.id }, addedStatus);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("status/{statusId}")]
        public async Task<IActionResult> DeleteJobStatus(int statusId)
        {
            try
            {
                await _jobService.DeleteJobStatus(statusId);
                return Ok("Job status deleted successfully");
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("statuses")]
        public async Task<ActionResult<IEnumerable<JobStatus>>> GetJobStatuses()
        {
            try
            {
                var statuses = await _jobService.GetJobStatuses();
                return Ok(statuses);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("status/{statusId}")]
        public async Task<ActionResult<JobStatus>> GetJobStatusByID(int statusId)
        {
            try
            {
                var status = await _jobService.GetJobStatusByID(statusId);
                return Ok(status);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<JobTypes>>> GetJobTypes()
        {
            try
            {
                var types = await _jobService.GetJobTypes();
                return Ok(types);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("type/{typeId}")]
        public async Task<ActionResult<JobTypes>> GetJobTypeByID(int typeId)
        {
            try
            {
                var type = await _jobService.GetJobTypeByID(typeId);
                return Ok(type);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPut("type")]
        public async Task<IActionResult> UpdateJobType([FromBody] JobTypes jobType)
        {
            try
            {
                var updatedType = await _jobService.UpdateJobType(jobType);
                return Ok(updatedType);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("type")]
        public async Task<IActionResult> AddJobType([FromBody] JobTypes jobType)
        {
            try
            {
                var addedType = await _jobService.AddJobType(jobType);
                return CreatedAtAction(nameof(GetJobTypeByID), new { typeId = addedType.id }, addedType);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("type/{typeId}")]
        public async Task<IActionResult> DeleteJobType(int typeId)
        {
            try
            {
                await _jobService.DeleteJobType(typeId);
                return Ok("Job type deleted successfully");
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}