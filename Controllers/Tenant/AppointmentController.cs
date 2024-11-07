using hoistmt.Models;
using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Globalization;
using hoistmt.Interfaces;
using System.Text.Json;

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
        //test

        [HttpGet("Appointments")]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointments([FromQuery] string startDate,
            [FromQuery] string endDate)
        {
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return NotFound("Tenant DbContext not available for the retrieved database.");
            }

            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return BadRequest("Start date and end date parameters are required.");
            }

            var startDateTime = DateTime.Parse(startDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
                .ToUniversalTime();
            var endDateTime = DateTime.Parse(endDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
                .ToUniversalTime();

            Console.WriteLine($"Fetching appointments between {startDateTime} and {endDateTime}");

            try
            {
                var query = from a in dbContext.appointments
                    join bs in dbContext.bookingstatus on a.BookingStatusID equals bs.id
                    join c in dbContext.customers on a.customerID equals c.id
                    join v in dbContext.vehicles on a.vehicleID equals v.id
                    where a.Active != 0 &&
                          a.start_time >= startDateTime &&
                          a.end_time <= endDateTime
                    select new
                    {
                        Id = a.id,
                        StartTime = a.start_time,
                        EndTime = a.end_time,
                        Notes = a.notes,
                        Active = a.Active,
                        LastModified = a.lastModified,
                        Jobs = a.Jobs, // Keep it as is
                        BookingStatus = new
                        {
                            Id = bs.id,
                            Title = bs.title,
                            Color = bs.color
                        },
                        InvoiceID = a.invoiceID,

                        Customer = new
                        {
                            Id = c.id,
                            Name = c.FirstName + ' ' + c.LastName,
                            Phone = c.Phone
                        },
                        Vehicle = new
                        {
                            Id = v.id,
                            Make = v.make,
                            Model = v.model,
                            Rego = v.rego
                        }
                    };

// Execute the query and materialize the result
                var bookings = await query.ToListAsync();

                // Serialize Jobs after the query
                var result = bookings.Select(b => new
                {
                    b.Id,
                    b.StartTime,
                    b.EndTime,
                    b.Notes,
                    b.Active,
                    b.LastModified,
                    Jobs = JsonSerializer.Serialize(b.Jobs), // Serialize here
                    b.BookingStatus,
                    b.InvoiceID,
                    b.Customer,
                    b.Vehicle
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bookings: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpPut("updateCustomer/{appointmentId}")]
        public async Task<IActionResult> UpdateAppointmentCustomer(int appointmentId,
            [FromBody] CustomerUpdateModel model)
        {
            try
            {
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
                var appointment = await dbContext.appointments.FindAsync(appointmentId);
                if (appointment == null)
                    return NotFound("Appointment not found");

                appointment.customerID = model.CustomerId;
                appointment.lastModified = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();

                return Ok("Customer updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("updateVehicle/{appointmentId}")]
        public async Task<IActionResult> UpdateAppointmentVehicle(int appointmentId, [FromBody] int vehicleId)
        {
            try
            {
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
                var appointment = await dbContext.appointments.FindAsync(appointmentId);
                if (appointment == null)
                    return NotFound("Appointment not found");

                appointment.vehicleID = vehicleId;
                appointment.lastModified = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();

                return Ok("Vehicle updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPut("update")]
        public async Task<bool> UpdateAppointment([FromBody] Appointment appointment)
        {
            try
            {
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
                // appointmentEntity.title = appointment.title;
                // appointmentEntity.backgroundColor = appointment.backgroundColor;
                appointmentEntity.start_time = appointment.start_time;
                appointmentEntity.end_time = appointment.end_time;

                appointmentEntity.notes = string.IsNullOrEmpty(appointment.notes) ? null : appointment.notes;
                appointmentEntity.lastModified = DateTime.Now;
                // appointmentEntity.eventID = appointment.eventID;
                appointmentEntity.Active = 1;


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
        public async Task<bool> AddAppointment([FromBody] Appointment appointment)
        {
            try
            {
                // Get the application db context
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();

                // Create a new Appointment entity
                var appointmentEntity = new Appointment
                {
                    // title = appointment.title,
                    // backgroundColor = appointment.backgroundColor,
                    start_time = appointment.start_time,
                    end_time = appointment.end_time,

                    notes = string.IsNullOrEmpty(appointment.notes) ? null : appointment.notes,
                    lastModified = DateTime.Now,
                    // eventID = appointment.eventID,
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

public class CustomerUpdateModel
{
    public int CustomerId { get; set; }
}