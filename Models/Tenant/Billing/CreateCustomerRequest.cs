namespace hoistmt.Models.Tenant.Billing;

public class CreateCustomerRequest
{
    public string Email { get; set; }
    public string PaymentMethodId { get; set; }
}
