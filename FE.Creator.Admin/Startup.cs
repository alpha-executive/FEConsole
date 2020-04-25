using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication;
using IdentityModel.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using IdentityModel;
using FE.Creator.AspNetCoreUtil;
using Microsoft.CodeAnalysis.Options;

namespace FE.Creator.Admin
{
    public class Startup
    {
        IWebHostEnvironment _env = null;

        public Startup(IConfiguration configuration,
            IWebHostEnvironment env)
        {
            this._env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            //add the physical file provider.
            var physicalProvider = _env.WebRootFileProvider;
            services.AddSingleton<IFileProvider>(physicalProvider);


            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddHttpClient("client")
                .AddHttpMessageHandler<UserAccessTokenHandler>();

            services.AddAccessTokenManagement(options =>
            {
                options.User.RefreshBeforeExpiration = TimeSpan.FromMinutes(5);
            })
            .ConfigureBackchannelHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(60);
            });

            
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            //Authentication support.
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
           .AddCookie("Cookies")
           .AddOpenIdConnect("oidc", "Identity Server4", options =>
           {
               options.Authority = Configuration.GetSection("Authentication:IdentityServer")
                                   .GetValue<string>("Url");
               options.RequireHttpsMetadata = true;

               options.ClientId = Configuration.GetSection("Authentication:IdentityServer")
                                   .GetValue<string>("ClientId");
               options.ClientSecret = Configuration.GetSection("Authentication:IdentityServer")
                                   .GetValue<string>("ClientSecret");
               options.ResponseType = Configuration.GetSection("Authentication:IdentityServer")
                                   .GetValue<string>("ResponseType");

               options.SaveTokens = true;

               options.Scope.Add("profile");
               options.Scope.Add("email");
               options.Scope.Add("offline_access");
               options.Scope.Add("feconsoleapi");

               options.ClaimActions.Add(new JsonKeyClaimAction(JwtClaimTypes.Name, null, JwtClaimTypes.Name));
               options.ClaimActions.Add(new JsonKeyClaimAction(JwtClaimTypes.Email, null, JwtClaimTypes.Email));
               options.GetClaimsFromUserInfoEndpoint = true;

               //options.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents()
               //{
               //    OnTokenResponseReceived = context =>
               //    {
               //        // Add the access_token as a claim, as we may actually need it
               //        var accessToken = context.TokenEndpointResponse.AccessToken;
               //        context.Request.Headers["access_token"] = accessToken;
               //        return Task.CompletedTask;
               //    },
               //    OnTicketReceived = async e =>
               //    {
               //        var token = e.Request.Headers["access_token"];
               //        var factory = e.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
               //        var client = factory.CreateClient("client");
               //        var authServerUrl = Configuration.GetSection("Authentication:IdentityServer")
               //                    .GetValue<string>("Url");

               //        var disco = await client.GetDiscoveryDocumentAsync(authServerUrl);
               //        client.SetBearerToken(token);
               //        var response = await client.GetUserInfoAsync(new UserInfoRequest
               //        {
               //            Address = disco.UserInfoEndpoint,
               //            Token = token
               //        });

               //        var name = response.Claims.FirstOrDefault(claim => claim.Type == "name");
               //        var email = response.Claims.FirstOrDefault(claim => claim.Type == "email");

               //        var claimsToKeep = new List<Claim> { name, email };

               //        var newIdentity = new ClaimsIdentity(claimsToKeep, e.Principal.Identity.AuthenticationType);

               //        e.Principal = new ClaimsPrincipal(newIdentity);
               //    }
               //};
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

                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.AddInitialRequestCultureProvider(new CustomRequestCultureProvider(async context =>
                {
                    var httpFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                    HttpClient client = httpFactory != null ?
                                                httpFactory.CreateClient("client") : new HttpClient();
                    string baseUrl = Configuration["SiteSettings:FEconsoleApiUrl"];

                    var token = await context.GetTokenAsync("access_token");
                    client.SetBearerToken(token);
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

                    var requestCulture = context.Features.Get<IRequestCultureFeature>();
                    // fail back to the request culture.
                    return new ProviderCultureResult(requestCulture != null ?
                        requestCulture.RequestCulture.UICulture.Name
                        : "en");
                }));
            });


            services.AddMvc()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                logger.LogInformation("Developer Exception page is Used.");
                //app.UseDeveloperExceptionPage();
                app.UseExceptionHandler("/Home/Error");
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                logger.LogInformation("Working in Production Mode.");
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseRequestLocalization();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                   name: "FEConsoleArea",
                   pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                .RequireAuthorization();
            });
        }
    }
}
