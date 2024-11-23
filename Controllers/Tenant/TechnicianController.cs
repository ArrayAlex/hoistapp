using hoistmt.Models;
using hoistmt.Services;
using hoistmt.Services.lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hoistmt.Exceptions;
using hoistmt.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using hoistmt.Models.Tenant;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechnicianController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private readonly TechnicianService _TechnicianService;

        public TechnicianController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, TechnicianService TechnicianService)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _TechnicianService = TechnicianService;
        }
        
        [HttpGet("Technicians")]
        public async Task<ActionResult<IEnumerable<UserAccount>>> GetTechnicians()
        {
            try
            {
                var Technicians = await _TechnicianService.GetTechniciansAsync();
                return Ok(Technicians);
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
        
        // [HttpGet("Technician")]
        // public async Task<ActionResult<IEnumerable<Technician>>> GetTechnician([FromQuery] int appointmentId)
        // {
        //     try
        //     {
        //         var Technicians = await _TechnicianService.GetTechniciansByAppointmentId(appointmentId);
        //         return Ok(Technicians);
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
        //
        // [HttpPost("add")]
        // public async Task<IActionResult> AddTechnician([FromBody] Technician Technician)
        // {
        //     try
        //     {
        //         var addedTechnician = await _TechnicianService.AddTechnicianAsync(Technician);
        //         return CreatedAtAction(nameof(GetTechnicianDetails), new { id = addedTechnician.TechnicianId }, addedTechnician);
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
        //
        // [HttpPut("Technician/{id}")]
        // public async Task<IActionResult> GetTechnicianDetails(int id)
        // {
        //     try
        //     {
        //         var Technician = await _TechnicianService.GetTechnicianDetails(id);
        //         if (Technician == null)
        //         {
        //             return NotFound($"Technician with ID {id} not found.");
        //         }
        //         return Ok(Technician);
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
        //
        // [HttpPut("Technicianboardid/{TechnicianId}")]
        // public async Task<IActionResult> UpdateTechnicianDetails(int TechnicianId, [FromBody] int newTechnicianBoardId)
        // {
        //     try
        //     {
        //         var Technician = await _TechnicianService.UpdateTechnicianBoardIdAsync(TechnicianId, newTechnicianBoardId);
        //         if (Technician == null)
        //         {
        //             return NotFound($"Technician with ID {TechnicianId} not found.");
        //         }
        //         return Ok(Technician);
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
        //
        //
        // // [HttpGet("search")]
        // // public async Task<ActionResult<IEnumerable<Technician>>> SearchTechnicians([FromQuery] string searchTerm)
        // // {
        // //     try
        // //     {
        // //         var Technicians = await _TechnicianService.SearchTechnicians(searchTerm);
        // //         return Ok(Technicians);
        // //     }
        // //     catch (UnauthorizedException ex)
        // //     {
        // //         return Unauthorized(ex.Message);
        // //     }
        // //     catch (Exception ex)
        // //     {
        // //         System.Diagnostics.Trace.WriteLine($"Error searching Technicians: {ex.Message}");
        // //         return StatusCode(500, "Internal Server Error");
        // //     }
        // // }
        //
        // [HttpGet("customer/{customerId}")]
        // public async Task<IActionResult> GetTechniciansByCustomerId(int customerId)
        // {
        //     try
        //     {
        //         var Technicians = await _TechnicianService.GetTechniciansByCustomerId(customerId);
        //         if (!Technicians.Any())
        //         {
        //             return NotFound($"No Technicians found for customer ID {customerId}");
        //         }
        //         return Ok(Technicians);
        //     }
        //     catch(UnauthorizedException ex)
        //     {
        //         return Unauthorized(ex.Message);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, "An error occurred while processing your request.");
        //     }
        // }
        //
        // [HttpDelete("delete/{TechnicianId}")]
        // public async Task<IActionResult> DeleteTechnician(int TechnicianId)
        // {
        //     try
        //     {
        //         await _TechnicianService.DeleteTechnician(TechnicianId);
        //         return Ok("Technician deleted successfully");
        //     }
        //     catch (NotFoundException ex)
        //     {
        //         return NotFound(ex.Message);
        //     }
        //     catch (UnauthorizedException ex)
        //     {
        //         return Unauthorized(ex.Message);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, "Internal Server Error: " + ex.Message);
        //     }
        // }
        //
        // [HttpPut("update")]
        // public async Task<IActionResult> UpdateTechnician([FromBody] Technician Technician)
        // {
        //     try
        //     {
        //         var updatedTechnician = await _TechnicianService.UpdateTechnician(Technician);
        //         return Ok(updatedTechnician);
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
        //         return StatusCode(500, "Internal Server Error: " + ex.Message);
        //     }
        // }
        //
        // // [HttpPut("updateStatus/{TechnicianId}")]
        // // public async Task<IActionResult> UpdateTechnicianStatus(int TechnicianId, [FromBody] int statusId)
        // // {
        // //     try
        // //     {
        // //         var updatedTechnician = await _TechnicianService.UpdateTechnicianStatus(TechnicianId, statusId);
        // //         return Ok(updatedTechnician);
        // //     }
        // //     catch (UnauthorizedException ex)
        // //     {
        // //         return Unauthorized(ex.Message);
        // //     }
        // //     catch (NotFoundException ex)
        // //     {
        // //         return NotFound(ex.Message);
        // //     }
        // //     catch (Exception ex)
        // //     {
        // //         return StatusCode(500, "An error occurred while processing your request.");
        // //     }
        // // }
        //
        // [HttpPost("status")]
        // public async Task<IActionResult> AddTechnicianStatus([FromBody] TechnicianStatus TechnicianStatus)
        // {
        //     try
        //     {
        //         var addedStatus = await _TechnicianService.AddTechnicianStatus(TechnicianStatus);
        //         return CreatedAtAction(nameof(GetTechnicianStatusByID), new { statusId = addedStatus.id }, addedStatus);
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
        //
        // [HttpDelete("status/{statusId}")]
        // public async Task<IActionResult> DeleteTechnicianStatus(int statusId)
        // {
        //     try
        //     {
        //         await _TechnicianService.DeleteTechnicianStatus(statusId);
        //         return Ok("Technician status deleted successfully");
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
        //
        // [HttpGet("statuses")]
        // public async Task<ActionResult<IEnumerable<TechnicianStatus>>> GetTechnicianStatuses()
        // {
        //     try
        //     {
        //         var statuses = await _TechnicianService.GetTechnicianStatuses();
        //         return Ok(statuses);
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
        //
        // [HttpGet("status/{statusId}")]
        // public async Task<ActionResult<TechnicianStatus>> GetTechnicianStatusByID(int statusId)
        // {
        //     try
        //     {
        //         var status = await _TechnicianService.GetTechnicianStatusByID(statusId);
        //         return Ok(status);
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
        //
        // [HttpGet("types")]
        // public async Task<ActionResult<IEnumerable<TechnicianTypes>>> GetTechnicianTypes()
        // {
        //     try
        //     {
        //         var types = await _TechnicianService.GetTechnicianTypes();
        //         return Ok(types);
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
        //
        // [HttpGet("type/{typeId}")]
        // public async Task<ActionResult<TechnicianTypes>> GetTechnicianTypeByID(int typeId)
        // {
        //     try
        //     {
        //         var type = await _TechnicianService.GetTechnicianTypeByID(typeId);
        //         return Ok(type);
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
        //
        // [HttpPut("type")]
        // public async Task<IActionResult> UpdateTechnicianType([FromBody] TechnicianTypes TechnicianType)
        // {
        //     try
        //     {
        //         var updatedType = await _TechnicianService.UpdateTechnicianType(TechnicianType);
        //         return Ok(updatedType);
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
        //
        // [HttpPost("type")]
        // public async Task<IActionResult> AddTechnicianType([FromBody] TechnicianTypes TechnicianType)
        // {
        //     try
        //     {
        //         var addedType = await _TechnicianService.AddTechnicianType(TechnicianType);
        //         return CreatedAtAction(nameof(GetTechnicianTypeByID), new { typeId = addedType.id }, addedType);
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
        //
        // [HttpDelete("type/{typeId}")]
        // public async Task<IActionResult> DeleteTechnicianType(int typeId)
        // {
        //     try
        //     {
        //         await _TechnicianService.DeleteTechnicianType(typeId);
        //         return Ok("Technician type deleted successfully");
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
    }
}