using hoistmt.Models;
using hoistmt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hoistmt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;

        public BookingController(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
        {
            _tenantDbContextResolver = tenantDbContextResolver;
        }

        // GET: api/Booking
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            var context = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (context == null)
            {
                return NotFound("Not logged in.");
            }
            return await context.Bookings.ToListAsync();
        }

        // GET: api/Booking/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingDetails(int id)
        {
            var context = await _tenantDbContextResolver.GetTenantDbContextAsync();
            var booking = await context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }

        // POST: api/Booking
        [HttpPost]
        public async Task<IActionResult> AddBooking([FromBody] Booking booking)
        {
            var context = await _tenantDbContextResolver.GetTenantDbContextAsync();
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBookingDetails), new { id = booking.Id }, booking);
        }

        // PUT: api/Booking
        [HttpPut]
        public async Task<IActionResult> UpdateBooking([FromBody] Booking booking)
        {
            var context = await _tenantDbContextResolver.GetTenantDbContextAsync();
            context.Entry(booking).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Booking/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var context = await _tenantDbContextResolver.GetTenantDbContextAsync();
            var booking = await context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            context.Bookings.Remove(booking);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
