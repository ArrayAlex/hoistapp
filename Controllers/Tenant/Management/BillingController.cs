using hoistmt.Functions;
using hoistmt.Models.Tenant.Billing;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Threading.Tasks;
using hoistmt.Services;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace hoistmt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillingController : ControllerBase
    {
        private readonly StripeService _stripeService;
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;

        public BillingController(StripeService stripeService, ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver)
        {
            _stripeService = stripeService;
            _tenantDbContextResolver = tenantDbContextResolver;
        }

        [HttpPost("create-customer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            var customer = await _stripeService.CreateCustomerAsync(request.Email, request.PaymentMethodId);
            return Ok(customer);
        }

        [HttpPost("create-subscription")]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
        {
            var subscription = await _stripeService.CreateSubscriptionAsync(request.CustomerId, request.PriceId);
            return Ok(subscription);
        }

        [HttpPost("add-payment-method")]
        public async Task<IActionResult> AddPaymentMethod([FromBody] AddPaymentMethodRequest request)
        {
            try
            {
                var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
                if (dbContext == null)
                {
                    return NotFound("Tenant DbContext not available for the retrieved database.");
                }

                var customer = await _stripeService.CreateCustomerAsync(request.Email, request.PaymentMethodId);
                var paymentMethod = await _stripeService.GetPaymentMethodAsync(request.PaymentMethodId);

                // Save payment method details to the database
                var paymentGateway = new PaymentGateway
                {
                    MethodId = paymentMethod.Id,
                    Card = paymentMethod.Card.Last4,
                    Active = true,
                    CustomerId = customer.Id,
                    Brand = paymentMethod.Card.Brand,
                    Last4 = paymentMethod.Card.Last4
                };

                dbContext.paymentgateway.Add(paymentGateway);
                await dbContext.SaveChangesAsync();

                return Ok(new 
                {
                    CustomerId = customer.Id,
                    PaymentMethodId = paymentMethod.Id,
                    paymentMethod.Card.Last4,
                    paymentMethod.Card.Brand
                });
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = e.Message });
            }
        }

        [HttpGet("payment-methods")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return NotFound("Tenant DbContext not available for the retrieved database.");
            }

            var paymentMethods = await dbContext.paymentgateway
                .Where(pg => pg.Active)
                .Select(pg => new 
                {
                    pg.MethodId,
                    pg.Card,
                    pg.Brand,
                    pg.Last4
                })
                .ToListAsync();

            return Ok(paymentMethods);
        }
    }

    public class CreateCustomerRequest
    {
        public string Email { get; set; }
        public string PaymentMethodId { get; set; }
    }

    public class CreateSubscriptionRequest
    {
        public string CustomerId { get; set; }
        public string PriceId { get; set; }
    }

    public class AddPaymentMethodRequest
    {
        public string Email { get; set; }
        public string PaymentMethodId { get; set; }
    }
}
