using Microsoft.AspNetCore.Http;
using FE.Creator.AspNetCoreUtil;
namespace FE.Creator.PCenter.CutomProvider
{
    public static class LoginUserHelper
    {
       public static bool IsUserLogin(this HttpContext context){
           return context != null  
                && context.User.Identity != null
                && context.User.Identity.IsAuthenticated
                && !string.IsNullOrEmpty(context.GetLoginUserEmail());
       }

       public static string GetLoginUserDisplayName(this HttpContext context) {
           if(context.IsUserLogin()){
                return context.GetLoginUser();
           }

           return string.Empty;
       }
    }
}