// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FE.Creator.IdentityServer.Data;
using FE.Creator.IdentityServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Globalization;

namespace FE.Creator.IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // configures IIS out-of-proc settings (see https://github.com/aspnet/AspNetCore/issues/14882)
            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            // configures IIS in-proc settings
            services.Configure<IISServerOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Csp.Level = IdentityServer4.Models.CspLevel.One;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddInMemoryIdentityResources(Config.Ids)
                .AddInMemoryApiResources(Config.Apis)
                .AddInMemoryClients(Config
                                        .Clients(Configuration.GetSection("Authentication:IdentityServer:Clients")))
                //.AddInMemoryClients (section)
                .AddAspNetIdentity<ApplicationUser>();

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddAuthentication()
                .AddMicrosoftAccount("Microsoft", microsoftOptions =>
                {
                    //microsoftOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    IConfigurationSection authSection = Configuration.GetSection("Authentication:Microsoft");
                    microsoftOptions.ClientId = authSection["externalauth-microsoft-clientid"];
                    microsoftOptions.ClientSecret = authSection["externalauth-microsoft-secret"];
                })
                .AddGoogle("Google", googleOptions =>
                {
                    //googleOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    IConfigurationSection authSection = Configuration.GetSection("Authentication:Google");
                    googleOptions.ClientId = authSection["externalauth-google-clientid"];
                    googleOptions.ClientSecret = authSection["externalauth-google-secret"];
                })
                .AddGitHub("Github", githubOptions => {
                    IConfigurationSection authSection = Configuration.GetSection("Authentication:Github");
                    githubOptions.ClientId = authSection["externalauth-github-clientid"];
                    githubOptions.ClientSecret = authSection["externalauth-github-secret"];
                    githubOptions.Scope.Add("read:user");
                    githubOptions.Scope.Add("user:email");
                });


            //Localization support.
            services.AddLocalization(options => options.ResourcesPath = "Lang");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("en"),
                        new CultureInfo("zh")
                    };
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.AddMvc()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseRequestLocalization();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}