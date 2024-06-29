using Stripe;
using System.Threading.Tasks;
using hoistmt.Models.MasterDbModels.Stripe;
using hoistmt.Data;
using hoistmt.Services;
using Microsoft.EntityFrameworkCore;

namespace hoistmt.Functions
{
    public class StripeService
    {
        private readonly string _secretKey;
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;

        public StripeService(IConfiguration configuration, ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
        {
            _secretKey = configuration["Stripe:SecretKey"];
            StripeConfiguration.ApiKey = _secretKey;
            _tenantDbContextResolver = tenantDbContextResolver;
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

        public async Task<PaymentIntent> CreatePaymentIntentAsync(string customerId, decimal amount, string paymentMethodId)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert amount to cents
                Currency = "nzd",
                Customer = customerId,
                PaymentMethod = paymentMethodId,
                Confirm = true, // Automatically confirm the payment
                OffSession = true, // Specify if the payment is off-session if needed
            };

            var service = new PaymentIntentService();
            try
            {
                return await service.CreateAsync(options);
            }
            catch (StripeException ex)
            {
                // Handle Stripe exceptions appropriately
                throw new ApplicationException($" API error: {ex.Message}", ex);
            }
        }

        public async Task DeletePaymentMethodAsync(int paymentGatewayId)
        {
            var tenantDbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (tenantDbContext == null)
            {
                throw new ApplicationException("Tenant DbContext not available.");
            }

            // Find the payment method in the database
            var paymentMethod = await tenantDbContext.paymentgateway.FirstOrDefaultAsync(pm => pm.Id == paymentGatewayId);
            if (paymentMethod == null)
            {
                throw new ApplicationException("Payment method not found.");
            }

            // Remove payment method from Stripe
            var service = new PaymentMethodService();
            await service.DetachAsync(paymentMethod.MethodId);

            // Remove payment method from the database
            tenantDbContext.paymentgateway.Remove(paymentMethod);
            await tenantDbContext.SaveChangesAsync();
        }
    }
}
