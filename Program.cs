using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DatingApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var host = CreateHostBuilder(args).Build();
            CreateHostBuilder(args).Build().Run();
            // using (var scope = host.Services.CreateScope())
            // {
            //     var services = scope.ServiceProvider;
            //     try
            //     {
            //         var context = services.GetRequiredService<DataContext>();
            //         context.Database.Migrate();
            //         Seed.SeedUsers(context);
            //     }
            //     catch (Exception ex)
            //     {
            //         var logger = services.GetRequiredService<ILogger<Program>>();
            //         logger.LogError(ex, "An error occured during migration");
            //     }
            // }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
