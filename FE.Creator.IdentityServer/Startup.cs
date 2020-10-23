// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FE.Creator.FEConsole.Shared.Models;
using FE.Creator.IdentityServer.Data;
using FE.Creator.IdentityServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace FE.Creator.IdentityServer
{
    public class Startup
    {
        ReverseProxyConfig reverseProxyConfig = null;
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
            reverseProxyConfig = new ReverseProxyConfig();
            configuration.GetSection("SiteSettings:ReverseProxy")
                         .Bind(reverseProxyConfig);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.Configure<CookiePolicyOptions>(options =>
           {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
               options.CheckConsentNeeded = context => true;
               options.MinimumSameSitePolicy = SameSiteMode.None;
               options.Secure = CookieSecurePolicy.Always;
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
                bool forceHttps = Configuration.GetValue<bool>("SiteSettings:ForceHttps");
                if(forceHttps)
                {
                    app.UseHsts();
                    app.UseHttpsRedirection();
                }
            }

            app.UseStaticFiles();
            var cookiePolicyValue = Configuration.GetValue<string>("SiteSettings:CookieSecurePolicy");
            app.UseCookiePolicy(new CookiePolicyOptions() { Secure = Enum.Parse<CookieSecurePolicy>(cookiePolicyValue) });

            app.UseRouting();
            app.UseRequestLocalization();
            if (reverseProxyConfig?.Enabled == true)
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }
            app.UseIdentityServer();

        
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}