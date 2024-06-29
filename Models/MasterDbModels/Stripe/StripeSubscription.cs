namespace hoistmt.Models.MasterDbModels.Stripe;


    public class StripeSubscription
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string Status { get; set; }
        public long Created { get; set; }
        // Add other properties as needed
    }

