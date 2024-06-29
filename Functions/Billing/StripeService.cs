using Stripe;
using System.Threading.Tasks;
using hoistmt.Models.MasterDbModels.Stripe;

namespace hoistmt.Functions
{
    public class StripeService
    {
        private readonly string _secretKey;

        public StripeService(IConfiguration configuration)
        {
            _secretKey = configuration["Stripe:SecretKey"];
            StripeConfiguration.ApiKey = _secretKey;
        }

        public async Task<Customer> CreateCustomerAsync(string email, string paymentMethodId)
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                PaymentMethod = paymentMethodId,
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = paymentMethodId,
                },
            };
            var service = new CustomerService();
            return await service.CreateAsync(options);
        }

        public async Task<Stripe.Subscription> CreateSubscriptionAsync(string customerId, string priceId)
        {
            var options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions { Price = priceId },
                },
                Expand = new List<string> { "latest_invoice.payment_intent" },
            };
            var service = new SubscriptionService();
            return await service.CreateAsync(options);
        }

        public async Task<PaymentMethod> GetPaymentMethodAsync(string paymentMethodId)
        {
            var service = new PaymentMethodService();
            return await service.GetAsync(paymentMethodId);
        }
    }
}