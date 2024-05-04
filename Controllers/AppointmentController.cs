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
    public class AppointmentController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;


        public AppointmentController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
        {
            _tenantDbContextResolver = tenantDbContextResolver;

        }


        [HttpGet("Appointments")]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments([FromQuery] string startDate,
            [FromQuery] string endDate, [FromQuery] string token)
        {
            // Use TenantDbContextResolver to get the tenant-specific DbContext
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return NotFound("Tenant DbContext not available for the retrieved database.");
            }

            // Convert string parameters to DateTime objects
            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return BadRequest("Start date and end date parameters are required.");
            }

            var startDateTime = DateTime.Parse(startDate, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var endDateTime = DateTime.Parse(endDate, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            try
            {
                // Retrieve appointments with related event attributes
                var appointmentsWithAttributes = await dbContext.appointments
                    .Where(a => a.start_time >= startDateTime && a.end_time <= endDateTime && a.Active != 0) // Add condition to filter out appointments with Active set to 0
                    .Select(a => new
                    {
                        Id = a.id,
                        Title = a.title,
                        StartTime = a.start_time,
                        EndTime = a.end_time,
                        Description = a.description,
                        Notes = a.notes,
                        EventAttribute = dbContext.eventAttributes
                            .Where(ea => ea.id == a.eventID)
                            .Select(ea => new
                            {
                                // Select only the desired columns from eventAttributes table
                                //id, title, start_time, end_time, description, notes, backgroundColor, borderColor, textColor, editable, startEditable, durationEditable, resourceEditable, display, overlap, constraint, allDay, classNames, url, extendedProps
                                ea.id,
                                ea.title,
                                ea.backgroundColor,
                                ea.borderColor,
                                ea.textColor,
                                // Add more columns as needed
                            })
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                // If no appointments found, return an empty array
                if (appointmentsWithAttributes == null || !appointmentsWithAttributes.Any())
                {
                    return Ok(new List<Appointment>());
                }

                return Ok(appointmentsWithAttributes);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error fetching appointments: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpPut("update")]
        public async Task<bool> UpdateAppointment([FromQuery] string token, Appointment appointment)
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
                appointmentEntity.eventID = appointment.eventID;
                appointmentEntity.Active = 1;

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
        public async Task<bool> AddAppointment([FromQuery] string token, Appointment appointment)
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
                    eventID = appointment.eventID,
                    Active = 1
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

        [HttpDelete("delete/{appointmentId}")]
        public async Task<bool> DeleteAppointment([FromQuery] string token, int appointmentId)
        {
            try
            {
                // Get the application db context
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();

                // Find the appointment to delete
                var appointmentEntity = await dbContext.appointments.FindAsync(appointmentId);
                if (appointmentEntity == null)
                {
                    // Appointment not found, return false
                    return false;
                }

                // Set Active to 0 to mark it as deleted
                appointmentEntity.Active = 0;
                appointmentEntity.lastModified = DateTime.Now;

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