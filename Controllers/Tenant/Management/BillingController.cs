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
using System.Text.Json;
using hoistmt.Exceptions;
using hoistmt.Interfaces;
using hoistmt.Models.Billing;
using hoistmt.Services.lib;
using Exception = System.Exception;

namespace hoistmt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillingController : ControllerBase
    {
        private BillingService _billingService;

        public BillingController(BillingService billingService)
        {
            _billingService = billingService;
        }

        [HttpPost("create-customer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            try
            {
                var customer = await _billingService.CreateCustomer(request);
                return Ok(customer);
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = e.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("payBill")]
        public async Task<IActionResult> PayBill([FromBody] PayBillRequest request)
        {
            try
            {
                var payBill = await _billingService.PayBill(request);
                return Ok(payBill);
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(e.Message);
            }
            catch (StripeException e)
            {
                return BadRequest("");
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest("bad request");
            }
        }

        [HttpPost("create-subscription")]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
        {
            try
            {
                var createSubscription = await _billingService.CreateSubscription(request);
                return Ok(createSubscription);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost("deletePaymentMethod")]
        public async Task<IActionResult> DeletePaymentMethod([FromBody] DeletePaymentMethodRequest request)
        {
            try
            {
                var deletePaymentMethod = await _billingService.DeletePaymentMethod(request);
                return Ok();
            }
            catch (ApplicationException e)
            {
                return BadRequest("error");
            }
            catch (Exception e)
            {
                return BadRequest("error");
            }
        }

        [HttpPost("add-payment-method")]
        public async Task<IActionResult> AddPaymentMethod([FromBody] AddPaymentMethodRequest request)
        {
            try
            {
                var addPaymentMethod = await _billingService.AddPaymentMethod(request);
                return Ok(addPaymentMethod);
            }
            catch (StripeException e)
            {
                return BadRequest("error");
            }
            catch (Exception e)
            {
                return BadRequest("error");
            }
        }

        [HttpGet("payment-methods")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            try
            {
                var paymentMethods = await _billingService.GetPaymentMethods();
                return Ok(paymentMethods);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest("error");
            }
        }

        [HttpPost("set-default-payment-method")]
        public async Task<IActionResult> SetDefaultPaymentMethod([FromBody] SetDefaultPaymentMethodRequest request)
        {
            try
            {
                var setDefaultPaymentMethod = await _billingService.SetDefaultPaymentMethod(request);
                return Ok(setDefaultPaymentMethod);
            }
            catch (NotFoundException e)
            {
                return NotFound("Payment method not found");
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest("error");
            }
        }

        [HttpGet("billing-info")]
        public async Task<IActionResult> getBillingInfo()
        {
            try
            {
                var billinginfo = await _billingService.GetBillingInfo();
                return Ok(billinginfo);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest("error");
            }
        }

        [HttpPut("update-billing-info")]
        public async Task<IActionResult> UpdateBillingInfo([FromBody] AccountBillingInfo request)
        {
            try
            {
                var updateBillingInfo = await _billingService.UpdateBillingInfo(request);
                return Ok(updateBillingInfo);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest("error");
            }
        }
    }
}