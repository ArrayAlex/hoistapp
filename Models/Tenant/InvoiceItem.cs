using System.ComponentModel.DataAnnotations;

namespace hoistmt.Models;

public class InvoiceItem
{
    public int InvoiceItemID { get; set; }
    public int invoice_id { get; set; }
    public string ItemType { get; set; } // Job, AdHocEntry
    public int ItemID { get; set; }
    public decimal Amount { get; set; }
}