using hoistmt.Data;
using hoistmt.Models;
using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks; // Add this namespace for Task

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
        private readonly ApplicationDbContext _context;

        public EventsController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver,
            ApplicationDbContext context)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
            _context = context;
        }


        [HttpGet("Events")]
        public async Task<ActionResult<IEnumerable<eventAttribute>>> GetEvents([FromQuery] int? id, [FromQuery] string token)
        {
            // Use TenantDbContextResolver to get the tenant-specific DbContext
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return NotFound("Tenant DbContext not available for the retrieved database.");
            }

            try
            {
                IQueryable<eventAttribute> query = dbContext.eventAttributes;

                // If ID is provided, filter by that ID
                if (id.HasValue)
                {
                    query = query.Where(ea => ea.id == id.Value);
                }

                var events = await query.ToListAsync();

                // If no events found, return appropriate response
                if (events == null || !events.Any())
                {
                    return NotFound("No events found.");
                }

                return Ok(events);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error fetching events: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }



        [HttpPut("update")]
        public async Task<bool> UpdateEvents([FromQuery] string token, Appointment appointment)
        {
            try
            {
            
                Console.WriteLine("############################################################");
                Console.WriteLine(appointment.id);
                Console.WriteLine(appointment.title);
                Console.WriteLine(appointment.backgroundColor);
                Console.WriteLine(appointment.start_time);
                Console.WriteLine(appointment.end_time);
                Console.WriteLine(appointment.description);
                Console.WriteLine(appointment.notes);
                
                // Get the application db context
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();

                // Update the appointment in the database
                var appointmentEntity = await dbContext.appointments.FindAsync(appointment.id);
                if (appointmentEntity == null)
                {
                    // Appointment not found, return false
                    return false;
                }


                // Update properties of the appointment entity
                appointmentEntity.title = appointment.title;
                appointmentEntity.backgroundColor = appointment.backgroundColor;
                appointmentEntity.start_time = appointment.start_time;
                appointmentEntity.end_time = appointment.end_time;
                appointmentEntity.description =
                    string.IsNullOrEmpty(appointment.description) ? null : appointment.description;
                appointmentEntity.notes = string.IsNullOrEmpty(appointment.notes) ? null : appointment.notes;
                appointmentEntity.lastModified = DateTime.Now;

                Console.WriteLine(appointmentEntity.start_time);
                Console.WriteLine(appointmentEntity.end_time);

                // Save changes to the database
                await dbContext.SaveChangesAsync();

                // If update succeeds without exceptions, return true
                return true;
            }
            catch (Exception ex)
            {
                // If any exceptions occur during update, return false
                return false;
            }
        }

        [HttpPost("add")]
        public async Task<bool> AddEvents([FromQuery] string token, Appointment appointment)
        {
            try
            {
                // Get the application db context
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();

                // Create a new Appointment entity
                var appointmentEntity = new Appointment
                {
                    title = appointment.title,
                    backgroundColor = appointment.backgroundColor,
                    start_time = appointment.start_time,
                    end_time = appointment.end_time,
                    description = string.IsNullOrEmpty(appointment.description) ? null : appointment.description,
                    notes = string.IsNullOrEmpty(appointment.notes) ? null : appointment.notes,
                    lastModified = DateTime.Now,
                    eventID = appointment.eventID
                };

                // Add the new appointment to the appointments DbSet
                dbContext.appointments.Add(appointmentEntity);

                // Save changes to the database
                await dbContext.SaveChangesAsync();

                // If insertion succeeds without exceptions, return true
                return true;
            }
            catch (Exception ex)
            {
                // If any exceptions occur during insertion, return false
                return false;
            }
        }

        [HttpDelete("delete/{eventid}")]
        public async Task<bool> DeleteAppointment([FromQuery] string token, int eventid)
        {
            try
            {
                // Get the application db context
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();

                // Find the appointment to delete
                var eventAttribute = await dbContext.eventAttributes.FindAsync(eventid);
                if (eventAttribute == null)
                {
                    // Appointment not found, return false
                    return false;
                }

                // Set Active to 0 to mark it as deleted
                // eventAttribute.Active = 0;
                //eventAttribute.lastModified = DateTime.Now;

                // Save changes to the database
                await dbContext.SaveChangesAsync();

                // If deletion succeeds without exceptions, return true
                return true;
            }
            catch (Exception ex)
            {
                // If any exceptions occur during deletion, return false
                return false;
            }
        }
    }
}