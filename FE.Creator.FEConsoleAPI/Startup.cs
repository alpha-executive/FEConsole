using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using FE.Creator.Cryptography;
using FE.Creator.FEConsole.Shared.Models;
using FE.Creator.FEConsole.Shared.Models.FileStorage;
using FE.Creator.FEConsole.Shared.Services.Cryptography;
using FE.Creator.FEConsole.Shared.Services.FileStorage;
using FE.Creator.FEConsoleAPI.MVCExtension;
using FE.Creator.FileStorage;
using FE.Creator.ObjectRepository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

namespace FE.Creator.FEConsoleAPI
{
    public class Startup
    {
        private static readonly string FEAPIAllowSpecificOrigins = "_FEAPIAllowSpecificOrigins";
        ReverseProxyConfig reverseProxyConfig = null;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            reverseProxyConfig = new ReverseProxyConfig();
            configuration.GetSection("SiteSettings:ReverseProxy")
                         .Bind(reverseProxyConfig);
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

                string widthConfig = Configuration["SiteSettings:FileStorageService:Thumbinal:Width"] ?? "260";
                string heighConfig = Configuration["SiteSettings:FileStorageService:Thumbinal:Height"] ?? "260";
                ThumbinalConfig thumbinalConfig = new ThumbinalConfig()
                {
                    Width = int.Parse(widthConfig),
                    Height = int.Parse(heighConfig)
                };

                var resxFileProvider = new EmbededResourceStorageProvider();
                var fileStorageService = new DefaultFileStorageService(fileProvider,
                    thumbinalConfig);
                fileStorageService.AddStorageProvider(resxFileProvider);

                return fileStorageService;
            });
            services.AddSingleton<IObjectService>(new DefaultObjectService());
            services.AddSingleton<IRSACryptographyService>(CryptographyServiceFactory.RSACryptoService);
            services.AddSingleton<ISymmetricCryptographyService>(CryptographyServiceFactory.SymmetricCryptoService);
            services.AddSingleton<ISHAService>(CryptographyServiceFactory.SHAService);

            services.AddCors(options =>
            {
                List<string> cors = new List<string>();
                Configuration.Bind("SiteSettings:CORS", cors);
                options.AddPolicy(name: FEAPIAllowSpecificOrigins,
                                  builder =>
                                  {
                                      builder
                                        .AllowAnyMethod()
                                        .AllowAnyHeader()
                                        .WithOrigins(cors.ToArray());
                                  });
            });

            if (reverseProxyConfig?.Enabled == true)
            {
                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    foreach (var ip in reverseProxyConfig.AllowedIPAddress)
                    {
                        options.KnownProxies.Add(IPAddress.Parse(ip));
                    }
                });
            }


            services.AddAuthentication("Bearer")
             .AddJwtBearer("Bearer", options =>
             {
                 options.Authority = Configuration.GetSection("Authentication:IdentityServer")
                                   .GetValue<string>("Url");
                 options.RequireHttpsMetadata = Configuration.GetSection("Authentication:IdentityServer")
                                   .GetValue<bool>("RequireHttpsMetadata");

                 //options.RequireHttpsMetadata = false;

                 options.Audience = "feconsoleapi";
             });

            //memory cache.
            services.AddMemoryCache();

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
                app.UseHsts();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                bool forceHttps = Configuration.GetValue<bool>("SiteSettings:ForceHttps");
                if(forceHttps)
                {
                    app.UseHsts();        
                }
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors(FEAPIAllowSpecificOrigins);

            if (reverseProxyConfig?.Enabled == true)
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                         .RequireAuthorization();
            });
        }
    }
}
