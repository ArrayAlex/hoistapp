using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hoistmt.Models
{
    public class JobRequest
    {

        public int JobId { get; set; }

        public int CustomerId { get; set; }

        public int VehicleId { get; set; }


        public int? TechnicianId { get; set; }

        public string? Notes { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int? JobStatus { get; set; }

 
        public int? JobType { get; set; }
      
        public int? JobBoardID { get; set; }
        

        public int? AppointmentId { get; set; }

        public int? CreatedBy { get; set; }
    }
}