using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FE.Creator.ObjectRepository;
using FE.Creator.PCenter.MvcExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace coreaspnet
{
    public class Startup
    {
        private static readonly string ANGULAR_XSRF_TOKEN = "X-XSRF-TOKEN";
        private  readonly ILoggerFactory  _loggerFactory = null;
        private  readonly IConfiguration _configuration = null;
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get {return _configuration;} }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var logger = _loggerFactory.CreateLogger<Startup>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //XSRF support in angular
            services.AddAntiforgery(options=> options.HeaderName = ANGULAR_XSRF_TOKEN);
            logger.LogInformation("XSRF for angular script is added to Http header.");

            services.AddSingleton<IObjectService, DefaultObjectService>();
            logger.LogInformation("IObjectService is added to as singleton service");

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            logger.LogInformation("DOTNET Compatiblity version is 2.1");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
        }
    }
}
