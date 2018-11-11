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
                        .AddJsonFile("appsettings.json")
                        .Build();

            return config;
        }

        protected  override void  OnModelCreating(ModelBuilder modelBuilder){
            //define self reference group.
            modelBuilder.Entity<GeneralObjectDefinitionGroup>()
                .HasOne(o => o.ParentGroup)
                .WithMany(o => o.ChildrenGroups)
                .HasForeignKey("ParentGroupID");

            modelBuilder.Entity<GeneralObjectDefinitionSelectItem>()
                        .Property(p=>p.GeneralObjectDefinitionSelectItemID)
                        .ValueGeneratedOnAdd();
            
            modelBuilder.Entity<GeneralObjectField>()
            .HasOne(g=>g.GeneralObject)
            .WithMany(o=>o.GeneralObjectFields)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FileGeneralObjectField>();
            modelBuilder.Entity<GeneralObjectReferenceField>();
            modelBuilder.Entity<ObjRefObjectDefinitionField>();
            modelBuilder.Entity<PrimeGeneralObjectField>();
            modelBuilder.Entity<PrimeObjectDefintionField>();
            modelBuilder.Entity<SingleSelectionDefinitionField>();
            modelBuilder.Entity<SingleSelectionGeneralObjectField>();

        }
    }
}