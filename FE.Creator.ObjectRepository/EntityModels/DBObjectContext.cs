using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace FE.Creator.ObjectRepository.EntityModels
{
    public abstract class DBObjectContext : DbContext
    {   
        internal DbSet<GeneralObjectDefinition> GeneralObjectDefinitions { get; set; }
        internal DbSet<GeneralObjectDefinitionGroup> GeneralObjectDefinitionGroups { get; set; }

        internal DbSet<GeneralObject> GeneralObjects { get; set; }

        internal DbSet<GeneralObjectField> GeneralObjectFields { get; set; }

        internal DbSet<GeneralObjectDefinitionField> GeneralObjectDefinitionFields { get; set; }

        internal DbSet<GeneralObjectDefinitionSelectItem> GeneralObjectDefinitionSelectItems { get; set; }
        internal DBObjectContext(){}
        internal DBObjectContext(DbContextOptions options):base(options){} 
        
        protected IConfiguration GetAppConfigure(){
            var config = new ConfigurationBuilder()
                        .AddJsonFile("appconfig.json")
                        .Build();

            return config;
        }

    }
}