using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hoistmt.Models
{
    public class Booking
    {
        [Key]
        
        public int Id { get; set; }

        [Required]
        public string JobType { get; set; } // e.g., Repair, Maintenance, etc.

        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string VehicleDetails { get; set; } // e.g., Make, Model, Year

        [Required]
        public string ContactNumber { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public string Status { get; set; } // e.g., Scheduled, In Progress, Completed

        public string Notes { get; set; }

        // Foreign key to Tenant (assuming multi-tenancy setup)
        [Required]
        public int TenantId { get; set; }

        // Additional fields as necessary
    }
}