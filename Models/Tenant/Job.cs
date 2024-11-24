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

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("JobStatusID")]
        public int? JobStatusID { get; set; }

        [Column("JobTypeID")]
        public int? JobTypeID { get; set; }
        
        [Column("JobBoardID")]
        public int? JobBoardID { get; set; }
        
        [Column("hours_worked")]
        public int? hours_worked { get; set; }
        
        [Column("AppointmentId")]
        public int? AppointmentId { get; set; }
        
        [Column("created_by")]
        public int? CreatedBy { get; set; }
        
        [Column("updated_by")]
        public int? updated_by { get; set; }
        [Column("invoice_id")]
        public int? invoice_id { get; set; }
    }
}