using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace SignalrServer
{
    class Program
    {
        public static void Main(string[] args)
        {
            var services = CreateHostBuilder(args).Build();

            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            logger.Info($"Starting..");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel(options =>
                {
                    options.ListenAnyIP(52776);
                }).UseStartup<Startup>();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Subscriber>();
                services.AddSingleton<CacheContainer>();
                services.AddSingleton<IConfigurationRoot>(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build());

            }).ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
            }).UseNLog();
    }
}
