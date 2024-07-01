using hoistmt;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                    .ConfigureKestrel((context, options) =>
                    {
                        if (context.HostingEnvironment.IsDevelopment())
                        {
                            // For local development, listen on HTTP
                            options.ListenAnyIP(80);
                        }
                        else
                        {
                            // For production, listen on HTTPS
                            options.ListenAnyIP(443, listenOptions =>
                            {
                                listenOptions.UseHttps();
                            });
                        }
                    });
            });
}