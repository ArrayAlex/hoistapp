using System.ComponentModel.DataAnnotations;

namespace hoistmt.Models;

public class Invoice
{
    [Key]
    public int invoice_id { get; set; }
    public int customerid { get; set; }
    public DateOnly? invoice_date { get; set; }
    public decimal? total_amount { get; set; }
    public string payment_status { get; set; } // Use the enum directly here
    public int vehicle_id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

}