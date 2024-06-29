namespace hoistmt.Models.MasterDbModels;

public class PayBillRequest
{
    public int InvoiceID { get; set; }
    public string? MethodID { get; set; }  // Optional: Specify a payment method ID
}