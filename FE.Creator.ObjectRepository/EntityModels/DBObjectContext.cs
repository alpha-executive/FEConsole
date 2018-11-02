using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace FE.Creator.ObjectRepository.EntityModels
{
    public abstract class DBObjectContext : DbContext
    {
        protected IConfiguration GetAppConfigure(){
            var config = new ConfigurationBuilder()
                        .AddJsonFile("appconfig.json")
                        .Build();

            return config;
        }

    }
}