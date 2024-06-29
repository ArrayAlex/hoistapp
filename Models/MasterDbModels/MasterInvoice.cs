using System.ComponentModel.DataAnnotations;

namespace hoistmt.Models.MasterDbModels;

public class MasterInvoice
{
    [Key]
    public int InvoiceID { get; set; }
    public decimal Amount { get; set; }
    public DateTime InvoiceDate { get; set; }
    public int CompanyID { get; set; }
    public bool IsPaid { get; set; }
}