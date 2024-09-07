namespace hoistmt.Models.Tenant.Billing;

public class CreateSubscriptionRequest
{
    public string CustomerId { get; set; }
    public string PriceId { get; set; }
}