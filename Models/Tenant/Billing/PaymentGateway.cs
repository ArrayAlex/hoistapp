namespace hoistmt.Models.Tenant.Billing
{
    public class PaymentGateway
    {
        public int Id { get; set; }
        public string MethodId { get; set; }
        public string Card { get; set; }
        public bool Active { get; set; }
        public string CustomerId { get; set; } // Stripe Customer ID
        public string Brand { get; set; } // Card Brand
        public string Last4 { get; set; } // Last 4 digits of the card
        
        public bool Default { get; set; } 
    }
}