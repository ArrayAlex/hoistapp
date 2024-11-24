using System.ComponentModel.DataAnnotations;

namespace hoistmt.Models;

// Existing Invoice Model (keeps database naming)
public class Invoice
{
    [Key]
    public int invoice_id { get; set; }
    public int? customerid { get; set; }
    public DateOnly? invoice_date { get; set; }
    public decimal? total_amount { get; set; }
    public string? payment_status { get; set; } 
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    // Add navigation properties
   
}

// Related models matching your database


// public class AdHocEntry
// {
//     [Key]
//     public int adhoc_id { get; set; }
//     public int invoice_id { get; set; }
//     public string description { get; set; }
//     public decimal amount { get; set; }
//     
//     public Invoice Invoice { get; set; }
// }
//
// // DTOs for API responses
// public class InvoiceResponse
// {
//     public int invoice_id { get; set; }
//     public int? customerid { get; set; }
//     public DateOnly? invoice_date { get; set; }
//     public decimal? total_amount { get; set; }
//     public string payment_status { get; set; }
//     public DateTime created_at { get; set; }
//     public DateTime updated_at { get; set; }
//     public List<InvoiceItemResponse> items { get; set; } = new();
// }
//
// public class InvoiceItemResponse
// {
//     public string type { get; set; }  // "job" or "adhoc"
//     public int item_id { get; set; }  // job_id or adhoc_id
//     public string description { get; set; }
//     public decimal amount { get; set; }
// }
//
// public class InvoiceDTO
// {
//     public DateTime InvoiceDate { get; set; }
//     public DateOnly DueDate { get; set; }
//     public int? customerid { get; set; }
//     public string? Notes { get; set; }
//     public decimal TotalAmount { get; set; }
//     public List<InvoiceItemDTO> Items { get; set; }
// }
//
// public class InvoiceItemDTO
// {
//     public string Type { get; set; } // "job" or "adhoc"
//     public string notes { get; set; }
//     public decimal Amount { get; set; }
//     public int JobId { get; set; }
// }