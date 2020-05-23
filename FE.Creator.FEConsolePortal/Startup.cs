using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using IdentityModel.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FE.Creator.AspNetCoreUtil;
using IdentityModel.AspNetCore.AccessTokenManagement;
using FE.Creator.FEConsole.Shared.Models;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace FE.Creator.FEConsolePortal
{
    public class Startup
    {
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
            services.AddControllersWithViews();

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

            services.AddAccessTokenManagement(options =>
            {
                options.Client.Clients.Add("identityserver", new ClientCredentialsTokenRequest
                {
                    Address = Configuration["Authentication:IdentityServer:TokenEndPoint"],
                    ClientId = Configuration["Authentication:IdentityServer:ClientId"],
                    ClientSecret = Configuration["Authentication:IdentityServer:ClientSecret"],
                    Scope = "feconsoleapi"
                });
            });
            services.AddHttpClient("client")
                    .AddHttpMessageHandler<ClientAccessTokenHandler>();

            services.AddLocalization(options => options.ResourcesPath = "Lang");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("en"),
                        new CultureInfo("zh")
                    };

                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                //clear the default culture provider.
                options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(async context =>
                {
                    try
                    {
                        var httpFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        HttpClient client = httpFactory != null ?
                                                    httpFactory.CreateClient("client") : new HttpClient();
                        string baseUrl = Configuration["SiteSettings:FEconsoleApiUrl"];

                        //var token = await context.GetTokenAsync("access_token");
                        //client.SetBearerToken(token);
                        var lang = await client.GetSysConfiguredLanguage(baseUrl, null);
                        if (!string.IsNullOrEmpty(lang))
                        {
                            if ("zh-CN".Equals(lang, StringComparison.InvariantCultureIgnoreCase))
                            {
                                return new ProviderCultureResult("zh");
                            }
                            else
                            {
                                return new ProviderCultureResult("en");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    //var requestCulture = context.Features.Get<IRequestCultureFeature>();
                    // fail back to the request culture.
                    //return new ProviderCultureResult("en");
                    return null;
                }));
            });

            services.AddMvc()
           .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseExceptionHandler("/Home/Error");
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseRequestLocalization();

            if (reverseProxyConfig?.Enabled == true)
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
