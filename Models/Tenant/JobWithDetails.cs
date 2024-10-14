using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hoistmt.Models
{
   
    public class JobWithDetails
{
    [Key]
    public int JobId { get; set; }
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public int? TechnicianId { get; set; }
    public string Notes { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AppointmentId { get; set; }
    public JobStatusDetails JobStatus { get; set; }
    public JobTypeDetails JobType { get; set; }
}

public class JobStatusDetails
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Color { get; set; }
}

public class JobTypeDetails
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Color { get; set; }
}
}