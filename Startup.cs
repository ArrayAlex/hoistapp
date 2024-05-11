using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using hoistmt.Data;
using hoistmt.Middleware;
using hoistmt.Services;
using System;

namespace hoistmt
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                policy =>
                                {
                                    policy.WithOrigins("https://hoist.nz",  "http://localhost:5173") // Allow only hoist.nz origin
                                        .AllowAnyHeader()
                                        .AllowAnyMethod() 
                                        .AllowCredentials();    
                                });
            });

            services.AddSession(options =>
            {
                string environment = System.Environment.GetEnvironmentVariable("ENV");
                options.Cookie.Name = "HoistSession";
                if (environment == "production")
                {
                    Console.WriteLine("running in production enviroment");
                    options.Cookie.Domain = ".hoist.nz";
                }
                else
                {
                    Console.WriteLine("running in development enviroment");
                    options.Cookie.Domain = "localhost"; 
                }

               
                    
                
                options.IdleTimeout = TimeSpan.FromMinutes(120); // Set the session timeout duration

                // Get environment variables
                

                // You can use the retrieved environment variable value here or elsewhere in your code

                // Configure other session options as needed
            });
            services.AddControllersWithViews();
            services.AddHttpContextAccessor();
            // Add your database context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    Configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 23))));

            services.AddDbContextFactory<TenantDbContext>(options =>
                options.UseMySql(
                    Configuration.GetConnectionString("tenantConnection"),
                    new MySqlServerVersion(new Version(8, 0, 23))));

            // Register TenantDbContextResolver as a scoped service
            services.AddScoped(typeof(ITenantDbContextResolver<>), typeof(TenantDbContextResolver<>));
            services.AddScoped<TenantService>();
            services.AddScoped<TokenHandler>();


            services.AddSingleton<JwtService>(provider =>
            {
                var secretKey = "whatever12312312313asdasd2d2dw2d2wd";
                var issuer = "whatever1231231231232wd2d2d2dwd2wdw2dwd2";
                return new JwtService(secretKey, issuer);
            });


        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(MyAllowSpecificOrigins);


            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.Headers.Append("Access-Control-Allow-Origin", "https://hoist.nz");
                    context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
                    context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
                    context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                    await context.Response.CompleteAsync();
                }
                else
                {
                    await next();
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();
            /*var ignoredEndpoints = new List<string>
            {
                "/api/auth/login", // Add endpoints to ignore here
                "/api/tenant/register", // Add endpoints to ignore here
                // Add more endpoints as needed
            };


            
            app.UseMiddleware<SessionAuthenticationMiddleware>(ignoredEndpoints);*/

            //app.UseMiddleware(typeof(RequestLoggingMiddleware));
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}