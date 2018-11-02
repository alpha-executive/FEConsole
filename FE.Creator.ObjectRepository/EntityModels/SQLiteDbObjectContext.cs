namespace FE.Creator.ObjectRepository.EntityModels
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.EntityFrameworkCore.Sqlite;
    public class SQLiteDbObjectContext : DBObjectContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var config = GetAppConfigure();
            options.UseSqlite(config.GetConnectionString("sqliteDb"));
        }

    }
}