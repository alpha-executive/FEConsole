using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace FE.Creator.FileStorage
{
    [Table("StoredFile")]
    internal class StoredFile
    {
        [Key]
        public string fileUniqueName { get; set; }

        [NotNull]
        public string fileFullName { get; set; }

        public string fileFriendlyName { get; set; }

        [NotNull]
        public string fileCRC { get; set; }

        [NotNull]
        public Int64 fileSize { get; set; }

        public string fileThumbinalFullName { get; set; }
    }

    internal class SqliteLocalFileIndexDBContext : DbContext
    {
        private static readonly string STORED_FILE_CONN_STR = "storedfileconnstr";
        public DbSet<StoredFile> Files { get; set; }

        public SqliteLocalFileIndexDBContext()
            : base()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder()
                       .AddJsonFile("appsettings.json")
                       .Build();

            optionsBuilder.UseSqlite(config.GetConnectionString(STORED_FILE_CONN_STR));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<StoredFile>();
            //turn off the lazy loading to avoid load complete database into memory.
            //this.Configuration.LazyLoadingEnabled = false;
        }
    }
}
