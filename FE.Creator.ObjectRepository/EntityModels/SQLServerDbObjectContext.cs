namespace FE.Creator.ObjectRepository.EntityModels
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.SqlServer;
    using Microsoft.Extensions.Configuration;
    public class SQLServerDbObjectContext:DBObjectContext
    {
         protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var config = GetAppConfigure();
            options.UseSqlite(config.GetConnectionString("sqlServerDb"));
        }
    }
}