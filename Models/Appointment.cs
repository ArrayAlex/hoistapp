namespace hoistmt.Models;

public class Appointment
{
    //id, title, start_time, end_time, description, notes, Active, lastModified
    public int id { get; set; }
    public string title { get; set; }
    public DateTime start_time { get; set; }
    public DateTime end_time { get; set; }
    public string? description { get; set; }
    public string? notes { get; set; }
    public int Active { get; set; }
    public string? backgroundColor { get; set; }
    public int eventID { get; set; }
    public DateTime lastModified { get; set; }
}