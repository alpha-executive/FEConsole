using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using testconsole.EntityModels;

namespace testconsole
{
    public class UserInfoDBContext:DbContext
    {
         public virtual DbSet<UserInfo> users {get;set;}
         public virtual DbSet<Student>  students {get;set;}

        public UserInfoDBContext(){}
        public UserInfoDBContext(DbContextOptions<UserInfoDBContext> options):base(options){}
        


        private  IConfiguration SetupConfig(){
            var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

            return config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){
            IConfiguration config = SetupConfig();
            optionsBuilder.UseSqlServer(
                config.GetConnectionString("sqlServerDb")
            );

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder){
            modelBuilder.Entity<UserInfo>()
                        .Property(p=>p.UserName)
                        .IsRequired();

        }

        
    }
}