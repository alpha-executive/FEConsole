namespace FE.Creator.ObjectRepository.EntityModels
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.SqlServer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
    public class SQLServerDbObjectContext:DBObjectContext
    {
        private static ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                .AddConsole();
        });
        public SQLServerDbObjectContext(){}
        public SQLServerDbObjectContext(DbContextOptions<SQLServerDbObjectContext> options):base(options){}
         protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var config = DBObjectContext.GetAppConfigure();
            options
                .UseLoggerFactory(loggerFactory)
                .UseSqlServer(config.GetConnectionString("sqlServerDb"));
        }
    }
}