using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using hoistmt.Data;
using hoistmt.Services;
using hoistmt.Functions;
using hoistmt.HttpClients;
using hoistmt.Interfaces;
using hoistmt.Services.lib;
using StackExchange.Redis;

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
                    policy.WithOrigins("https://hoist.nz", "http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = Configuration.GetConnectionString("RedisConnection");
            options.InstanceName = "HoistSession_";
        });

        services.AddSession(options =>
        {
            options.Cookie.Name = "HoistSession";
            options.IdleTimeout = TimeSpan.FromMinutes(120);
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.None;
            
            options.Cookie.IsEssential = true;
            options.Cookie.Domain = Configuration["COOKIE_DOMAIN"] ?? "localhost";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.Path = "/";
        });

        services.AddControllersWithViews();
        services.AddHttpContextAccessor();
        services.AddHttpClient<RegoSearch>();

        services.AddDbContext<MasterDbContext>(options =>
            options.UseMySql(
                Configuration.GetConnectionString("MasterConnectionRemote"),
                new MySqlServerVersion(new Version(8, 0, 23))));

        services.AddDbContextFactory<TenantDbContext>(options =>
            options.UseMySql(
                Configuration.GetConnectionString("tenantConnectionRemote"),
                new MySqlServerVersion(new Version(8, 0, 23))));

        services.AddScoped(typeof(ITenantDbContextResolver<>), typeof(TenantDbContextResolver<>));
        services.AddScoped<TenantService>();
        services.AddScoped<TokenHandler>();
        services.AddScoped<Credits>();
        services.AddScoped<EmailService>();
        services.AddScoped<AccountSubscription >();
        services.AddScoped<StripeService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<AuthService>();
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var configuration = ConfigurationOptions.Parse(Configuration.GetConnectionString("RedisConnection"), true);
            return ConnectionMultiplexer.Connect(configuration);
        });
        services.AddSingleton<JwtService>(provider =>
        {
            var secretKey = Configuration["JWT_SECRET_KEY"] ?? "wabZ$Aa)]b7tF[[YvhqS*:dkzz9w";
            var issuer = Configuration["JWT_ISSUER"] ?? "KZ1^a<R8fMAhw^$kw[Cr^sNg5veR";
            return new JwtService(secretKey, issuer);
        });

        // services.AddTransient<DatabaseInitializer>();

        // Register background services
        // services.AddHostedService<GenerateInvoiceService>();
        // services.AddHostedService<ChargeInvoiceService>();
        // services.AddHostedService<DatabaseKeepAliveService>();
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
        app.UseCors("_HoistNZ");

        // app.Use(async (context, next) =>
        // {
        //     var origin = context.Request.Headers["Origin"].ToString();
        //     if (context.Request.Method == "OPTIONS")
        //     {
        //         if (origin == "https://hoist.nz" || origin == "http://localhost:3000")
        //         {
        //             context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
        //         }
        //         context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
        //         context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
        //         context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        //         await context.Response.CompleteAsync();
        //     }
        //     else
        //     {
        //         await next();
        //     }
        // });

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseSession();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });

        // var task = dbInitializer.InitializeAsync();
        // task.Wait();
    }
}

