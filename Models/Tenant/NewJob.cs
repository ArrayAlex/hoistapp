using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hoistmt.Models
{

    public class NewJob : JobBase
    {

        public int CustomerId { get; set; }

        public int VehicleId { get; set; }

        public int? TechnicianId { get; set; }

        public string? Notes { get; set; }

        public int? JobStatus { get; set; }

        public int? JobType { get; set; }

        public int? jobStatusId { get; set; }
        public int? jobTypeId { get; set; }
        public int? hours_worked { get; set; }

        public int? AppointmentId { get; set; }

        public int? CreatedBy { get; set; }
    }
    
    

    public abstract class JobBase
    {

        public int CustomerId { get; set; }


        public int VehicleId { get; set; }


        public int? TechnicianId { get; set; }

        public string? Notes { get; set; }


        public int? jobStatusId { get; set; }


        public int? jobTypeId { get; set; }


        public int? JobBoardID { get; set; }


        public int? AppointmentId { get; set; }


        public int? CreatedBy { get; set; }

        // Add created_at and updated_at fields

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}