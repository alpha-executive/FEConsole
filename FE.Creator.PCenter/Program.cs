using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.AspNetCore.Hosting;

namespace coreaspnet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            
            try
            {
                 Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                // uncomment to write to Azure diagnostics stream
                //.WriteTo.File(
                //    @"D:\home\LogFiles\Application\identityserver.txt",
                //    fileSizeLimitBytes: 1_000_000,
                //    rollOnFileSizeLimit: true,
                //    shared: true,
                //    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

                Log.Logger.Debug("Starting Web Application ...");

                var host = CreateHostBuilder(args).Build();
               /*  using(var scope = host.Services.CreateScope()){
                    var servieProvider = scope.ServiceProvider;
                } */
            
                host.Run();
                Log.Logger.Debug("Web Application Started!");

            }
            catch (System.Exception ex)
            {
                Log.Logger.Fatal(ex, "Unexpected Error Cause application stoped!");
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args){
            var builtConfig = new ConfigurationBuilder()
            .AddEnvironmentVariables("ASPNETCORE_")
            //.AddJsonFile("appsettings.json")
            .Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logoptions =>
                {
                    logoptions.AddSerilog();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddConfiguration(builtConfig);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
}
