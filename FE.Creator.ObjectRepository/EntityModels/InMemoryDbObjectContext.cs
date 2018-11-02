namespace FE.Creator.ObjectRepository.EntityModels
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.InMemory;
    public class InMemoryDbObjectContext:DBObjectContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseInMemoryDatabase(databaseName: "ferepository");
        }
    }
}