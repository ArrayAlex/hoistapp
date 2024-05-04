using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hoistmt.Data;
using hoistmt.Models;
using Microsoft.EntityFrameworkCore;

namespace hoistmt.Middleware
{
    public class SessionAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly List<string> _ignoredEndpoints;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionAuthenticationMiddleware(RequestDelegate next, List<string> ignoredEndpoints, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _ignoredEndpoints = ignoredEndpoints;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Invoke(HttpContext context, ApplicationDbContext dbContext)
        {
            // Check if the request path matches any of the ignored endpoints
            
            var requestPath = context.Request.Path.Value;
            if (_ignoredEndpoints.Any(endpoint => requestPath.StartsWith(endpoint)))
            {
                // Skip authentication for ignored endpoints
                await _next(context);
                return;
            }

            // Check if the request has a token query parameter
            if (context.Request.Query.TryGetValue("token", out var token) && !string.IsNullOrEmpty(token))
            {
                // Get the current UTC time
                
                var httpContext = _httpContextAccessor.HttpContext; // Retrieve HttpContext

                // Extract the tenant database schema name from the session
                var companyID = httpContext.Session.GetString("CompanyDb");
                // Check if session is valid based on the token and expiration time
              
                if (companyID == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }
            else
            {
                // Token not provided, return 401 Unauthorized
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await _next(context);
        }
    }
}
