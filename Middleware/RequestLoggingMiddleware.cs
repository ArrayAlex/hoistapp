using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using hoistmt.Data;
using hoistmt.Models;


namespace hoistmt.Middleware
{

    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Log the request details
            LogRequest(context.Request);

            // Call the next middleware in the pipeline
            await _next(context);
        }

        private void LogRequest(HttpRequest request)
        {
            // Log request method and path
          

            // Log request headers
            Console.WriteLine("Request Headers:");
            foreach (var header in request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }

            // Log request body (if present)
            if (request.ContentLength.HasValue && request.ContentLength > 0)
            {
                request.EnableBuffering();
                var requestBody = new StreamReader(request.Body).ReadToEnd();
                Console.WriteLine($"Request Body: {requestBody}");

                // Reset the stream position for downstream middleware
                request.Body.Position = 0;
            }
        }
    }
}