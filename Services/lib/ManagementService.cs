using hoistmt.Data;
using hoistmt.Exceptions;
using hoistmt.Interfaces;

namespace hoistmt.Services.lib;

public class ManagementService : IDisposable
{
    private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
    private bool  _disposed = false;
    private TenantDbContext _context;
    
    public ManagementService(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver, TenantDbContext context)
    {
        _tenantDbContextResolver =
            tenantDbContextResolver ?? throw new ArgumentNullException(nameof(tenantDbContextResolver));
        _context = context;
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