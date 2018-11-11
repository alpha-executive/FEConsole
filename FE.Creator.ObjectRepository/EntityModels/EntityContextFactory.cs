
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
        internal static DBObjectContext GetDBObjectContext(){
            return GetDBObjectContext(string.Empty, null);
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
                case "inmemory":
                    if(options != null)
                    {
                        return new InMemoryDbObjectContext((DbContextOptions<InMemoryDbObjectContext>)options);
                    }
                    else{
                        return new InMemoryDbObjectContext();
                    }
                default:
                     if(options != null)
                    {
                        return new SQLServerDbObjectContext((DbContextOptions<SQLServerDbObjectContext>)options);
                    }
                    else{
                        return new SQLServerDbObjectContext();
                    }
            }
        }
    }
}
