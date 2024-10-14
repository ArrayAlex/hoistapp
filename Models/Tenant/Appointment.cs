namespace hoistmt.Models;

public class Appointment
{
    public int id { get; set; }
    public DateTime start_time { get; set; }  // This should map directly from the JSON
    public DateTime end_time { get; set; }    // This should map directly from the JSON
    public string? notes { get; set; }
    public int Active { get; set; }
    public DateTime lastModified { get; set; }

    public List<string> Jobs { get; set; }
    public int? BookingStatusID { get; set; }
    public int? invoiceID { get; set; }
    public int? customerID { get; set; }
    public int? vehicleID { get; set; }
}
