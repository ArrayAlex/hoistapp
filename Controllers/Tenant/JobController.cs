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
                // Log the exception
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        
        [HttpPost("add")]
        public async Task<IActionResult> AddJob([FromBody] Job job)
        {
            try
            {
                var addedJob = await _jobService.AddJobAsync(job);
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
        
        [HttpGet("job/{id}")]
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

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Job>>> SearchJobs([FromQuery] string searchTerm)
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
    }
}