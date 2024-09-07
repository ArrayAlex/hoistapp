using hoistmt.Controllers;
using hoistmt.Data;
using hoistmt.Exceptions;
using hoistmt.Functions;
using hoistmt.Interfaces;
using hoistmt.Models.Billing;
using hoistmt.Models.MasterDbModels;
using hoistmt.Models.Tenant.Billing;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace hoistmt.Services.lib;

public class BillingService : IDisposable
{
    ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
    TenantDbContext _context;
    private readonly MasterDbContext _masterContext;
    private readonly StripeService _stripeService;
    
    private bool _disposed = false;

    public BillingService(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver,StripeService stripeService,  MasterDbContext masterContext)
    {
        _tenantDbContextResolver =
            tenantDbContextResolver ?? throw new ArgumentNullException(nameof(tenantDbContextResolver));
        _stripeService = stripeService;
        _masterContext = masterContext;
    }

    public async Task<Customer> CreateCustomer(CreateCustomerRequest request)
    {
        await EnsureContextInitializedAsync();
        var customer = await _stripeService.CreateCustomerAsync(request.Email, request.PaymentMethodId);
        return customer;
    }
    
    private async Task EnsureContextInitializedAsync()
    {
        if (_context == null)
        {
            _context = await _tenantDbContextResolver.GetTenantDbContextAsync();
            if (_context == null)
            {
                throw new UnauthorizedException("Not logged in or unauthorized access.");
            }
        }
    }

    public async Task<object> PayBill(PayBillRequest request)
    {
        await EnsureContextInitializedAsync();
        var invoice = await _masterContext.companyinvoices.FirstOrDefaultAsync(i => i.InvoiceID == request.InvoiceID);
        if (invoice == null)
        {
            throw new NotFoundException("not found");
        }
        PaymentGateway paymentMethod;
        if (string.IsNullOrEmpty(request.MethodID))
        {
            // Use the default payment method
            paymentMethod = await _context.paymentgateway.FirstOrDefaultAsync(pm => pm.Active && pm.Default);
        }
        else
        {
            // Use the specified payment method
            paymentMethod = await _context.paymentgateway.FirstOrDefaultAsync(pm => pm.MethodId == request.MethodID && pm.Active);
        }

        if (paymentMethod == null)
        {
            throw new NotFoundException("payment method not found");
        }
        try
        {
            var charge = await _stripeService.CreatePaymentIntentAsync(paymentMethod.CustomerId, invoice.Amount, paymentMethod.MethodId);
            return charge;
        }
        catch (StripeException e)
        {
            throw new StripeException("Stripe error");
        }
        
    }
    
    public async Task<object> CreateSubscription(CreateSubscriptionRequest request)
    {
        await EnsureContextInitializedAsync();
        var subscription = await _stripeService.CreateSubscriptionAsync(request.CustomerId, request.PriceId);
        return subscription;
    }

    public async Task<object> DeletePaymentMethod(DeletePaymentMethodRequest request)
    {
        try
        {
            await _stripeService.DeletePaymentMethodAsync(request.PaymentGatewayId);
            return new { Message = "Payment method deleted successfully" };
        }
        catch (ApplicationException ex)
        {
            throw new ApplicationException(ex.Message);
        }
        
    }

    public async Task<object> AddPaymentMethod(AddPaymentMethodRequest request)
    {
        try
        {
            await EnsureContextInitializedAsync();
            var customer = await _stripeService.CreateCustomerAsync(request.Email, request.PaymentMethodId);
            var paymentMethod = await _stripeService.GetPaymentMethodAsync(request.PaymentMethodId);

            var paymentGateway = new PaymentGateway
            {
                MethodId = paymentMethod.Id,
                Card = paymentMethod.Card.Last4,
                Active = true,
                CustomerId = customer.Id,
                Brand = paymentMethod.Card.Brand,
                Last4 = paymentMethod.Card.Last4
            };

            _context.paymentgateway.Add(paymentGateway);
            await _context.SaveChangesAsync();

            return new
            {
                CustomerId = customer.Id,
                PaymentMethodId = paymentMethod.Id,
                paymentMethod.Card.Last4,
                paymentMethod.Card.Brand
            };

        }
        catch (StripeException e)
        {
            throw new StripeException(e.Message);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    public async Task<IEnumerable<PaymentGateway>> GetPaymentMethods()
    {
        var paymentMethods = await _context.paymentgateway
            .Where(pg => pg.Active)
            .Select(pg => new PaymentGateway
            {
                MethodId = pg.MethodId,
                Card = pg.Card,
                Brand = pg.Brand,
                Last4 = pg.Last4,
                Id = pg.Id,
                Default = pg.Default
            })
            .ToListAsync();
        if (paymentMethods == null)
        {
            throw new NotFoundException("No payment methods found!");
        }
        return paymentMethods;
    }

    public async Task<bool> SetDefaultPaymentMethod(SetDefaultPaymentMethodRequest request)
    {
        await EnsureContextInitializedAsync();
        
        var paymentMethod =
            await _context.paymentgateway.FirstOrDefaultAsync(pm => pm.MethodId == request.MethodId);
        
        if (paymentMethod == null)
        {
            throw new NotFoundException("Payment Method not found");
        }
        
        var paymentMethods = await _context.paymentgateway.ToListAsync();
        foreach (var method in paymentMethods)
        {
            method.Default = false;
        }
        
        paymentMethod.Default = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<AccountBillingInfo>> GetBillingInfo()
    {
        await EnsureContextInitializedAsync();
        var billingInfo = await _context.company.AsNoTracking().ToListAsync();
        return billingInfo;

    }

    public async Task<AccountBillingInfo> UpdateBillingInfo(AccountBillingInfo accountBillingInfo)
    {
        await EnsureContextInitializedAsync();
        var billingInfo = await _context.company.FirstOrDefaultAsync();
        if (billingInfo == null)
        {
            throw new NotFoundException("Billing info not found.");
        }
        billingInfo.BusinessName = accountBillingInfo.BusinessName;
        billingInfo.AddressLine1 = accountBillingInfo.AddressLine1;
        billingInfo.City = accountBillingInfo.City;
        billingInfo.Country = accountBillingInfo.Country;
        
        await _context.SaveChangesAsync();
        return billingInfo;
    }
    
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context?.Dispose();
            }

            _disposed = true;
        }
    }
}