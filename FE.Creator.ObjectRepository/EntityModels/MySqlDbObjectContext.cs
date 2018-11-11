namespace FE.Creator.ObjectRepository.EntityModels
{
    using Microsoft.EntityFrameworkCore;
    public class MySqlDbObjectContext:DBObjectContext
    {
        public MySqlDbObjectContext(){}

        public MySqlDbObjectContext(DbContextOptions options):base(options){}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){
            var configure = GetAppConfigure();
        }
    }
}