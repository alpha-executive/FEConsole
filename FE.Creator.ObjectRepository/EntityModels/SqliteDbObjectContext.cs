namespace FE.Creator.ObjectRepository.EntityModels
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    public class SqliteDbObjectContext:DBObjectContext
    {
        public SqliteDbObjectContext(){}
        
        public SqliteDbObjectContext(DbContextOptions<SqliteDbObjectContext> options):base(options){}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){
            var config = DBObjectContext.GetAppConfigure();
            optionsBuilder.UseSqlite(config.GetConnectionString("sqliteDb"));
        }
    }
}