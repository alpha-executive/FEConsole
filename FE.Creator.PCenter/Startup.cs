using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Razor;
using IdentityModel.AspNetCore.AccessTokenManagement;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using IdentityModel;

namespace coreaspnet
{
    public class Startup
    {
        private static readonly string XSRF_TOKEN = "X-XSRF-TOKEN";
        private static readonly string CLIENT_XSRF_TOKEN_KEY = "XSRF-TOKEN";
        private  readonly IConfiguration _configuration = null;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration Configuration { get {return _configuration;} }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //var logger = _loggerFactory.CreateLogger<Startup>();
           // services.Configure<CookiePolicyOptions>(options =>
           //{
           //     // This lambda determines whether user consent for non-essential cookies is needed for a given request.
           //     options.CheckConsentNeeded = context => true;
           //    options.MinimumSameSitePolicy = SameSiteMode.None;
           //    options.Secure = CookieSecurePolicy.SameAsRequest;
           //});

            //XSRF support in angular
            services.AddAntiforgery(options=> options.HeaderName = XSRF_TOKEN);

            //Localization support.
            services.AddLocalization(options => options.ResourcesPath = "Lang");

            services.AddSession();
            /* services.AddSingleton<IObjectService, DefaultObjectService>();
            logger.LogInformation("IObjectService is added to as singleton service");
 */
             services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("en"),
                        new CultureInfo("zh")
                    };

                //options.DefaultRequestCulture = new RequestCulture("zh");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddHttpClient("client")
                .AddHttpMessageHandler<UserAccessTokenHandler>();

            services.AddAccessTokenManagement()
            .ConfigureBackchannelHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(60);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme =  "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", "Identity Server4", options =>
            {
                options.Authority = Configuration.GetSection("Authentication:IdentityServer")
                                   .GetValue<string>("Url");
                options.RequireHttpsMetadata = Configuration.GetSection("Authentication:IdentityServer")
                                   .GetValue<bool>("RequireHttpsMetadata");

                options.ClientId = Configuration.GetSection("Authentication:IdentityServer")
                                    .GetValue<string>("ClientId");
                options.ClientSecret = Configuration.GetSection("Authentication:IdentityServer")
                                    .GetValue<string>("ClientSecret");
                options.ResponseType = Configuration.GetSection("Authentication:IdentityServer")
                                    .GetValue<string>("ResponseType");
                options.SaveTokens = true;
                options.SignInScheme = "Cookies";

                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("offline_access");
                options.Scope.Add("feconsoleapi");

                options.ClaimActions.Add(new JsonKeyClaimAction(JwtClaimTypes.Name, null, JwtClaimTypes.Name));
                options.ClaimActions.Add(new JsonKeyClaimAction(JwtClaimTypes.Email, null, JwtClaimTypes.Email));
                options.GetClaimsFromUserInfoEndpoint = true;

            });
            services.AddControllersWithViews();
            services.AddMvc()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IAntiforgery antiforgery)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
            app.UseCookiePolicy();
            app.UseSession();
            app.UseRequestLocalization();

            app.UseRouting();
           
            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(next=> context =>
            {
                string path = context.Request.Path.Value;
                if (string.Equals(path, "/", StringComparison.OrdinalIgnoreCase))
                {
                    // The request token can be sent as a JavaScript-readable cookie, 
                    // and Angular uses it by default.
                    var tokens = antiforgery.GetAndStoreTokens(context);
                    context.Response.Cookies.Append(CLIENT_XSRF_TOKEN_KEY, tokens.RequestToken, 
                        new CookieOptions() { HttpOnly = false,  IsEssential = true, Secure = false });
                }

                return next(context);
            });

            app.UseEndpoints(endpoints =>
            {
                 endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
