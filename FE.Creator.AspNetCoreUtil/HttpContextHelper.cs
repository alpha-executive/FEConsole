using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;

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
    }
}
