using FE.Creator.Admin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FE.Creator.Admin
{
    public static class FEAdminHttpContextHelper
    {
        public static FileUploadSetting GetFileUploadSetting(this HttpContext httpContext)
        {
            var uploadSetting = new FileUploadSetting();
            var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();
            config.Bind("SiteSettings:FileUploadSetting",
                    uploadSetting);

            return uploadSetting;
        }
    }
}
