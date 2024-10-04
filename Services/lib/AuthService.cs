using hoistmt.Data;
using hoistmt.Interfaces;
using hoistmt.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;

namespace hoistmt.Services.lib;

public class AuthService : IDisposable
{
    private readonly ITenantDbContextResolver<TenantDbContext> _tenantDbContextResolver;
    private readonly MasterDbContext _masterDbContext;
    private readonly IConfiguration _configuration;
    private readonly JwtService _jwtService;
    private readonly TokenHandler _tokenHandler;
    private TenantDbContext _context;
    private bool _disposed = false;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(ITenantDbContextResolver<TenantDbContext> tenantDbContextResolver,
        MasterDbContext masterDbContext, IConfiguration configuration, JwtService jwtService, TokenHandler tokenHandler,
        IHttpContextAccessor httpContextAccessor)
    {
        _tenantDbContextResolver = tenantDbContextResolver;
        _configuration = configuration;
        _jwtService = jwtService;
        _tokenHandler = tokenHandler;
        _masterDbContext = masterDbContext;
        _tenantDbContextResolver =
            tenantDbContextResolver ?? throw new ArgumentNullException(nameof(tenantDbContextResolver));
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<object> Login(LoginModel login)
    {
        await EnsureCompanyContextInitializedAsync(login.Company);
        if (string.IsNullOrEmpty(login.Company) || string.IsNullOrEmpty(login.Password) ||
            string.IsNullOrEmpty(login.Username))
        {
            throw new InvalidRequest("Invalid login credentials");
        }

        var account = await _context.Set<UserAccount>()
            .FirstOrDefaultAsync(a =>
                a.Username == login.Username && a.Password == login.Password && a.IsVerified == true);

        if (account == null)
        {
            throw new InvalidRequest("Invalid username or password");
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext is not available.");
        }

        // Set session variables safely
        httpContext.Session.SetInt32("userid", account.Id);
        httpContext.Session.SetString("sessionid", httpContext.Session.Id);
        httpContext.Session.SetString("CompanyDb", login.Company);

        var company = await _masterDbContext.Companies.FirstOrDefaultAsync(c => c.CompanyID == login.Company);
        if (company == null)
        {
            throw new NotFoundException("Company not found");
        }

        var plan = await _masterDbContext.plansubscriptions.FirstOrDefaultAsync(p => p.id == company.PlanID);
        if (plan == null)
        {
            throw new NotFoundException("Plan not found");
        }

        

        httpContext.Session.SetInt32("PlanID", plan.id);
        httpContext.Session.SetString("PlanName", plan.PlanName);
        httpContext.Session.SetInt32("StorageLimitGB", plan.StorageLimitGB);
        httpContext.Session.SetInt32("MaxUsers", plan.MaxUsers);
        httpContext.Session.SetString("AccessFeatureA", plan.AccessFeatureA.ToString());
        httpContext.Session.SetString("AccessFeatureB", plan.AccessFeatureB.ToString());
        httpContext.Session.SetString("AccessFeatureC", plan.AccessFeatureC.ToString());
        httpContext.Session.SetString("AccessFeatureD", plan.AccessFeatureD.ToString());
        httpContext.Session.SetString("AccessFeatureE", plan.AccessFeatureE.ToString());

        var session = new Session
        {
            userID = account.Id,
            token = httpContext.Session.Id,
            ipAddress = httpContext.Connection.RemoteIpAddress.ToString(),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CompanyDb = httpContext.Session.GetString("CompanyDb")
        };

        _masterDbContext.sessions.Add(session);
        await _context.SaveChangesAsync();
        var newUser = await _masterDbContext.Companies.FirstOrDefaultAsync(c => c.CompanyID == login.Company);
        return new
        {
            newUser = newUser.New,
            Plan = new
            {
                PlanID = plan.id,
                PlanName = plan.PlanName,
                Cost = plan.MonthlyCost,
                StorageLimitGB = plan.StorageLimitGB,
                MaxUsers = plan.MaxUsers,
                AccessFeatureA = plan.AccessFeatureA,
                AccessFeatureB = plan.AccessFeatureB,
                AccessFeatureC = plan.AccessFeatureC,
                AccessFeatureD = plan.AccessFeatureD,
                AccessFeatureE = plan.AccessFeatureE
            }
            
        };
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

    private async Task EnsureCompanyContextInitializedAsync(string company)
    {
        if (_context == null)
        {
            _context = await _tenantDbContextResolver.GetTenantLoginDbContextAsync(company);
            if (_context == null)
            {
                throw new UnauthorizedException("Not logged in or unauthorized access.");
            }
        }
    }

    public async Task<object> VerifyToken()
    {
        if (_httpContextAccessor.HttpContext.Session.GetString("CompanyDb") == null)
        {
            throw new UnauthorizedException("Not logged in or unauthorized access.");
        }

        return new
        {
            Token = _httpContextAccessor.HttpContext.Session.Id,
            Plan = new
            {
                PlanID = _httpContextAccessor.HttpContext.Session.GetInt32("PlanID"),
                PlanName = _httpContextAccessor.HttpContext.Session.GetString("PlanName"),
                Cost = _httpContextAccessor.HttpContext.Session.GetInt32("StorageLimitGB"),
                MaxUsers = _httpContextAccessor.HttpContext.Session.GetInt32("MaxUsers"),
                AccessFeatureA = _httpContextAccessor.HttpContext.Session.GetString("AccessFeatureA"),
                AccessFeatureB = _httpContextAccessor.HttpContext.Session.GetString("AccessFeatureB"),
                AccessFeatureC = _httpContextAccessor.HttpContext.Session.GetString("AccessFeatureC"),
                AccessFeatureD = _httpContextAccessor.HttpContext.Session.GetString("AccessFeatureD"),
                AccessFeatureE = _httpContextAccessor.HttpContext.Session.GetString("AccessFeatureE")
            }
        };
    }

    public async Task<object> Logout()
    {
        try
        {
            // Clear all session variables
            _httpContextAccessor.HttpContext.Session.Clear();
            // Retrieve the session token from HttpContext
            var sessionId = _httpContextAccessor.HttpContext.Session.GetString("Token");
            var session = await _masterDbContext.sessions.FirstOrDefaultAsync(s => s.token == sessionId);
            if (session != null)
            {
                _masterDbContext.sessions.Remove(session);
                await _masterDbContext.SaveChangesAsync();
            }

            return new
            {
                Message = "Logout successful"
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"Error during logout: {ex.Message}");
            return new
            {
                Message = "An error occurred during logout"
            };
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