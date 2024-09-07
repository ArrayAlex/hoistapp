namespace hoistmt.Models.Tenant.Billing;

public class AddPaymentMethodRequest
{
    public string Email { get; set; }
    public string PaymentMethodId { get; set; }
}