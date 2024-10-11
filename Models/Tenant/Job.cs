using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hoistmt.Models
{
    [Table("jobs")]
    public class Job
    {
        [Key]
        [Column("job_id")]
        public int JobId { get; set; }

        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("vehicle_id")]
        public int VehicleId { get; set; }

        [Column("technician_id")]
        public int? TechnicianId { get; set; }

        [Column("invoice_id")]
        public int? InvoiceId { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("pickup_date")]
        public DateTime? PickupDate { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("notes")]
        public string Notes { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}