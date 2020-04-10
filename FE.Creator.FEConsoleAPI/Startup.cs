using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FE.Creator.Cryptography;
using FE.Creator.FEConsole.Shared.Services.Cryptography;
using FE.Creator.FEConsole.Shared.Services.FileStorage;
using FE.Creator.FEConsoleAPI.MVCExtension;
using FE.Creator.FileStorage;
using FE.Creator.ObjectRepository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

namespace FE.Creator.FEConsoleAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //register FEConsole Services.
            services.AddSingleton<IFileStorageService>(provider =>
            {
                string configuredPath = Configuration["SiteSettings:FileStoreProviders:FileSystem:RootDir"];
                DirectoryInfo directory = new DirectoryInfo(configuredPath);
                string rootFileDirectory = directory.FullName.Equals(configuredPath, StringComparison.InvariantCultureIgnoreCase) 
                        ? configuredPath :
                                Path.Combine(Environment.CurrentDirectory, configuredPath);
                var fileProvider = new FileSystemStorageProvider(rootFileDirectory);
                var fileStorageService = new DefaultFileStorageService(fileProvider);

                return fileStorageService;
            });
            services.AddSingleton<IObjectService>(new DefaultObjectService());
            services.AddSingleton<IRSACryptographyService>(CryptographyServiceFactory.RSACryptoService);
            services.AddSingleton<ISymmetricCryptographyService>(CryptographyServiceFactory.SymmetricCryptoService);

            services.AddControllers(options=>{
                            options.InputFormatters.Insert(0, new ObjectDefintionFormatter());
                    })
                    .AddNewtonsoftJson(options=>{
                        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                        //options.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects;
                        //options.SerializerSettings.Converters.Add()
                    });

            services.AddSwaggerDocument( swgOptions=>{
                  swgOptions.PostProcess = document =>
                    {
                        document.Info.Version = "v1";
                        document.Info.Title = "FEConsole API";
                        document.Info.Description = "API of the FEConsole Application.";
                        document.Info.TermsOfService = "None";
                        document.Info.Contact = new NSwag.OpenApiContact
                        {
                            Name = "FETECHLAB",
                            Email = string.Empty,
                            Url = "https://fetechlab.com/"
                        };
                        document.Info.License = new NSwag.OpenApiLicense
                        {
                            Name = "License",
                            Url = "https://www.apache.org/licenses/LICENSE-2.0"
                        };
                    };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
