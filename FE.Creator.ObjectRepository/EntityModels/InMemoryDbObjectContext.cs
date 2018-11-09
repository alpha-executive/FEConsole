namespace FE.Creator.ObjectRepository.EntityModels
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.InMemory;
    public class InMemoryDbObjectContext:DBObjectContext
    {
        public InMemoryDbObjectContext(){}
        public InMemoryDbObjectContext(DbContextOptions<InMemoryDbObjectContext> options):base(options){}
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseInMemoryDatabase(databaseName: "ferepository");
        }
    }
}