using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using hoistmt.Models.Tenant;


namespace hoistmt.Models;


public class InvoiceRequest
{
    public Invoice Invoice { get; set; }
}

public class Invoice
{
    [Key]
    public int invoice_id { get; set; }
    public int? customerid { get; set; }
    public string? Status { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public int? TaxRate { get; set; }
    public int? Discount { get; set; }
    public int? Subtotal { get; set; }
    public int? TaxAmount { get; set; }
    public int? DiscountAmount { get; set; }
    public int? Total { get; set; }
    public DateTime? created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? dueDate { get; set; }

    [ForeignKey("customerid")]
    public virtual Customer? Customer { get; set; }
    // [JsonIgnore]
    // [InverseProperty("Invoice")]
    public ICollection<LineItem> LineItems { get; set; }
}

