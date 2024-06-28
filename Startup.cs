
using Microsoft.EntityFrameworkCore;

using hoistmt.Data;

using hoistmt.Services;

using hoistmt.Functions;
using hoistmt.HttpClients;

namespace hoistmt;

public class Startup
{
    public IConfiguration Configuration { get; }
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: "_HoistNZ",
                policy =>
                {
                    policy.WithOrigins("https://hoist.nz", "http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        services.AddSession(options =>
        {
            options.Cookie.Name = "HoistSession";

            // Determine the domain based on the environment
            string cookieDomain = Configuration["COOKIE_DOMAIN"] ?? "localhost";
            options.Cookie.Domain = cookieDomain;

            options.IdleTimeout = TimeSpan.FromMinutes(120); // Set the session timeout duration

            // Configure other session options as needed
        });

        services.AddControllersWithViews();
        services.AddHttpContextAccessor();
        services.AddHttpClient<RegoSearch>();
        // Add your database context
        services.AddDbContext<MasterDbContext>(options =>
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
        services.AddScoped<Credits>();

        services.AddSingleton<JwtService>(provider =>
        {
            var secretKey = Configuration["JWT_SECRET_KEY"] ?? "wabZ$Aa)]b7tF[[YvhqS*:dkzz9w";
            var issuer = Configuration["JWT_ISSUER"] ?? "KZ1^a<R8fMAhw^$kw[Cr^sNg5veR";
            return new JwtService(secretKey, issuer);
        });

        services.AddTransient<DatabaseInitializer>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env,DatabaseInitializer dbInitializer)
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
        app.UseCors("_HoistNZ");


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

        var task = dbInitializer.InitializeAsync();
        task.Wait();
    }
}