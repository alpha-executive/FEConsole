
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.EntityModels
{
   internal class EntityContextFactory
    {
        protected static readonly string DEFAULT_DATABASE_PROVIDER = "sqlite";
        internal static DBObjectContext GetDBObjectContext(){
            var configuredDBProvider = DBObjectContext.GetConfiguredDBProvider();
            return GetDBObjectContext(string.IsNullOrEmpty(configuredDBProvider) ?
                DEFAULT_DATABASE_PROVIDER : configuredDBProvider, null);
        }

        internal static DBObjectContext GetDBObjectContext(string dbProvider, DbContextOptions options)
        {
            switch (dbProvider.ToLower())
            {
                case "sqlite":
                    if(options != null)
                    {
                        return new SqliteDbObjectContext((DbContextOptions<SqliteDbObjectContext>)options);
                    }
                    else{
                        return new SqliteDbObjectContext();
                    }
                case "mysql":
                    if(options != null)
                    {
                        return new MySqlDbObjectContext((DbContextOptions<MySqlDbObjectContext>)options);
                    }
                    else{
                        return new MySqlDbObjectContext();
                    }
                case "sqlserver":
                    if (options != null)
                    {
                        return new SQLServerDbObjectContext((DbContextOptions<SQLServerDbObjectContext>)options);
                    }
                    else
                    {
                        return new SQLServerDbObjectContext();
                    }
                default:
                    if (options != null)
                    {
                        return new InMemoryDbObjectContext((DbContextOptions<InMemoryDbObjectContext>)options);
                    }
                    else
                    {
                        return new InMemoryDbObjectContext();
                    }
            }
        }
    }
}
