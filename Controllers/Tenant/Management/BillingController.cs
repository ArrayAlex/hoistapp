using hoistmt.Functions;
using hoistmt.Models.Tenant.Billing;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Threading.Tasks;
using hoistmt.Services;
using System.Linq;
using hoistmt.Data;
using hoistmt.Models.MasterDbModels;
using Microsoft.EntityFrameworkCore;

namespace hoistmt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillingController : ControllerBase
    {
        private readonly MasterDbContext _context;
        private readonly StripeService _stripeService;
        private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;

        public BillingController(StripeService stripeService, ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, MasterDbContext context)
        {
            _stripeService = stripeService;
            _tenantDbContextResolver = tenantDbContextResolver;
            _context = context;
        }

        [HttpPost("create-customer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            var customer = await _stripeService.CreateCustomerAsync(request.Email, request.PaymentMethodId);
            return Ok(customer);
        }
        
        [HttpPost("payBill")]
        public async Task<IActionResult> PayBill([FromBody] PayBillRequest request)
        {
            // Retrieve tenant-specific database context
            var tenantDbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (tenantDbContext == null)
            {
                return NotFound("Tenant DbContext not available.");
            }

            // Fetch the invoice from the master database
            var invoice = await tenantDbContext.companyinvoices.FirstOrDefaultAsync(i => i.InvoiceID == request.InvoiceID);
            if (invoice == null)
            {
                return NotFound("Invoice not found.");
            }

            // Determine the payment method to use
            PaymentGateway paymentMethod;
            if (string.IsNullOrEmpty(request.MethodID))
            {
                // Use the default payment method
                paymentMethod = await tenantDbContext.paymentgateway.FirstOrDefaultAsync(pm => pm.Active && pm.Default);
            }
            else
            {
                // Use the specified payment method
                paymentMethod = await tenantDbContext.paymentgateway.FirstOrDefaultAsync(pm => pm.MethodId == request.MethodID && pm.Active);
            }

            if (paymentMethod == null)
            {
                return NotFound("Payment method not available.");
            }

            try
            {
                // Charge the card
                var charge = await _stripeService.CreatePaymentIntentAsync(paymentMethod.CustomerId, invoice.Amount, paymentMethod.MethodId);
                return Ok(new { Message = "Bill paid successfully" });
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = e.Message });
            }
        }

        [HttpPost("create-subscription")]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
        {
            var dbContext = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (dbContext == null)
            {
                return NotFound("Tenant DbContext not available for the retrieved database.");
            }
            var subscription = await _stripeService.CreateSubscriptionAsync(request.CustomerId, request.PriceId);
            return Ok(subscription);
        }
        
        [HttpPost("deletePaymentMethod")]
        public async Task<IActionResult> DeletePaymentMethod([FromBody] DeletePaymentMethodRequest request)
        {
            try
            {
                await _stripeService.DeletePaymentMethodAsync(request.PaymentGatewayId);
                return Ok(new { Message = "Payment method deleted successfully" });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
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
                    pg.Last4,
                    pg.Id
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

    public class DeletePaymentMethodRequest
    {
        public int PaymentGatewayId { get; set; }
    }
}
