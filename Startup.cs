using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
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

        // Configure Redis cache for session state
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = Configuration.GetConnectionString("RedisConnection");
            options.InstanceName = "HoistSession_";
        });

        // Configure session to use Redis
        services.AddSession(options =>
        {
            options.Cookie.Name = "HoistSession";
            options.IdleTimeout = TimeSpan.FromMinutes(120);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Domain = Configuration["COOKIE_DOMAIN"] ?? "localhost";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        services.AddControllersWithViews();
        services.AddHttpContextAccessor();
        services.AddHttpClient<RegoSearch>();

        // Choose the correct connection string based on the presence of COOKIE_DOMAIN
        string masterConnection = string.IsNullOrEmpty(Configuration["COOKIE_DOMAIN"])
            ? Configuration.GetConnectionString("masterConnectionLocal")
            : Configuration.GetConnectionString("masterConnectionRemote");

        string tenantConnection = string.IsNullOrEmpty(Configuration["COOKIE_DOMAIN"])
            ? Configuration.GetConnectionString("tenantConnectionLocal")
            : Configuration.GetConnectionString("tenantConnectionRemote");

        // Add your database context
        services.AddDbContext<MasterDbContext>(options =>
            options.UseMySql(
                masterConnection,
                new MySqlServerVersion(new Version(8, 0, 23))));

        services.AddDbContextFactory<TenantDbContext>(options =>
            options.UseMySql(
                tenantConnection,
                new MySqlServerVersion(new Version(8, 0, 23))));

        // Register TenantDbContextResolver as a scoped service
        services.AddScoped(typeof(ITenantDbContextResolver<>), typeof(TenantDbContextResolver<>));
        services.AddScoped<TenantService>();
        services.AddScoped<TokenHandler>();
        services.AddScoped<Credits>();
        services.AddScoped<AccountSubscription>();
        services.AddSingleton<StripeService>();
        services.AddSingleton<JwtService>(provider =>
        {
            var secretKey = Configuration["JWT_SECRET_KEY"] ?? "wabZ$Aa)]b7tF[[YvhqS*:dkzz9w";
            var issuer = Configuration["JWT_ISSUER"] ?? "KZ1^a<R8fMAhw^$kw[Cr^sNg5veR";
            return new JwtService(secretKey, issuer);
        });

        services.AddTransient<DatabaseInitializer>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DatabaseInitializer dbInitializer)
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
        app.UseSession(); // Ensure this is before UseRouting
        app.UseRouting();

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