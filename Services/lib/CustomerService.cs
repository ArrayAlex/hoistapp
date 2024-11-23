using hoistmt.Exceptions;
using hoistmt.Interfaces;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Customer = hoistmt.Models.Customer;

namespace hoistmt.Services.lib;

public class CustomerService : IDisposable
{
    private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
    private TenantDbContext _context;
    private bool _disposed = false;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public CustomerService(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, IHttpContextAccessor httpContextAccessor)
    {
        _tenantDbContextResolver =
            tenantDbContextResolver ?? throw new ArgumentNullException(nameof(tenantDbContextResolver));
        _httpContextAccessor = httpContextAccessor;
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

    public async Task<IEnumerable<Customer>> SearchCustomers(string searchTerm)
    {
        await EnsureContextInitializedAsync();
        
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetCustomers();

        var customers = await _context.customers
            .Where(c => c.FirstName.ToLower().Contains(searchTerm.ToLower()) ||
                        c.LastName.ToLower().Contains(searchTerm.ToLower()) ||
                        c.Email.ToLower().Contains(searchTerm.ToLower()) ||
                        c.Phone.Contains(searchTerm))
            .ToListAsync();

        System.Diagnostics.Debug.WriteLine($"Search term: {searchTerm}");
        System.Diagnostics.Debug.WriteLine($"Customers found: {customers.Count}");

        if (!customers.Any())
        {
            throw new NotFoundException($"No customers found matching '{searchTerm}'.");
        }

        return customers;
    }

    public async Task<IEnumerable<Customer>> GetCustomers()
    {
        await EnsureContextInitializedAsync();
        var customers = await _context.customers.ToListAsync();
        if (!customers.Any())
        {
            throw new NotFoundException("No customers found.");
        }

        return customers;
    }

    public async Task<Customer> GetCustomerById(int customerId)
    {
        await EnsureContextInitializedAsync();
        
        // Fetch the customer by ID
        var customer = await _context.customers.FirstOrDefaultAsync(c => c.id == customerId);
        
        // If customer is not found, throw an exception
        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID {customerId} not found.");
        }

        return customer;
    }

    public async Task<Customer> GetCustomerByVehicleId(int vehicleId)
    {
        await EnsureContextInitializedAsync();
        var customerId = await _context.vehicles.Where(v => v.id == vehicleId).Select(v => v.customerid)
            .FirstOrDefaultAsync();
        if (customerId == null)
        {
            throw new NotFoundException("No customer found for the vehicle.");
        }

        var customer = await _context.customers.FirstOrDefaultAsync(c => c.id == customerId);

        return customer;
    }

    public async Task<Customer> UpdateCustomer(Customer customer)
    {
        await EnsureContextInitializedAsync();
        var existingCustomer = await _context.customers.FirstOrDefaultAsync(c => c.id == customer.id);
        var userid = _httpContextAccessor.HttpContext.Session.GetInt32("userid");
        
        existingCustomer.FirstName = customer.FirstName;
        existingCustomer.LastName = customer.LastName;
        existingCustomer.Email = customer.Email;
        existingCustomer.Phone = customer.Phone;
        existingCustomer.updated_by = userid;
        
        await _context.SaveChangesAsync();
        return existingCustomer;
    }

    public async Task<Customer> AddCustomer(Customer customer)
    {
        await EnsureContextInitializedAsync();
        var userid = _httpContextAccessor.HttpContext.Session.GetInt32("userid");
        var CustomerEntity = new Customer
        {
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            updated_by = userid,
            created_by = userid
        };
        
        _context.customers.Add(CustomerEntity);

        // Save changes to the database
        await _context.SaveChangesAsync();

        return CustomerEntity;

    }
    
    public async Task<Customer> DeleteCustomer(int customerId)
    {
        await EnsureContextInitializedAsync();
        var customer = await _context.customers.FindAsync(customerId);
        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID {customerId} not found.");
        }

        _context.customers.Remove(customer);
        await _context.SaveChangesAsync();
        return customer;
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