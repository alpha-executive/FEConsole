using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Localization;
using System.Threading;

namespace FE.Creator.AspNetCoreUtil
{
    public static class HttpContextHelper
    {
        public static string GetLoginUser(this HttpContext httpContext)
        {
            var uname = string.Empty;
            uname = TryGetLoginUserCliamValue(httpContext, "name");
            if (string.IsNullOrEmpty(uname))
            {
                uname = GetLoginUserEmail(httpContext);
            }

            return uname;
        }

        private static string TryGetLoginUserCliamValue(HttpContext httpContext, string cliamKey)
        {
            if (httpContext.User != null
                && httpContext.User.Identity != null
                && httpContext.User.Identity.IsAuthenticated)
            {
                var cliam = httpContext
                                .User
                                .Claims
                                .FirstOrDefault((claim) =>
                                {
                                    return claim.Type == cliamKey;
                                });

                if(cliam != null)
                {
                    return cliam.Value;
                }
            }

            return string.Empty;
        }

        public static string GetCurrentCulture(this HttpContext httpContext)
        {
            var rqf = httpContext.Features.Get<IRequestCultureFeature>();
            // Culture contains the information of the requested culture
            var culture = rqf.RequestCulture.Culture;

            return culture != null ? culture.Name :
                        Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
        }


        public static string GetLoginUserEmail(this HttpContext httpContext)
        {
            var email = string.Empty;

            email = TryGetLoginUserCliamValue(httpContext, "email");

            return email;
        } 

        public static string GetLoginUserId(this HttpContext httpContext)
        {
            var userId = TryGetLoginUserCliamValue(httpContext, JwtClaimTypes.Subject)
                            ?? TryGetLoginUserCliamValue(httpContext, ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(JwtClaimTypes.Subject);
            }

            return userId;
        }

        public static string WebApiBaseUrl(this HttpContext httpContext)
        {
            var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();

            return config["SiteSettings:FEconsoleApiUrl"];
        }

        public static string FEPortalUrl(this HttpContext httpContext)
        {
            var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();

            return config["SiteSettings:FEPortalUrl"];
        }

        public static T GetConfigValue<T>(this HttpContext httpContext, string configKeyPath)
        {
            var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();

            return config.GetValue<T>(configKeyPath);
        }
    }
}
