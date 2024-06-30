using System.ComponentModel.DataAnnotations;

namespace hoistmt.Models.MasterDbModels;

public class CompanInvoice
{
    [Key]
    public int InvoiceID { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; }
    public string CompanyID { get; set; }
    public bool IsPaid { get; set; }
}