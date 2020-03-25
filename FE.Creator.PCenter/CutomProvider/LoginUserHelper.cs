using Microsoft.AspNetCore.Http;

namespace FE.Creator.PCenter.CutomProvider
{
    public static class LoginUserHelper
    {
       public static bool IsUserLogin(this HttpContext context){
           return context != null  
                && context.User.Identity != null
                && context.User.Identity.IsAuthenticated
                && !string.IsNullOrEmpty(context.Session.GetString("UserEmail"));
       }

       public static string GetLoginUserDisplayName(this HttpContext context) {
           if(context.IsUserLogin()){
               return 
                 string.IsNullOrEmpty(context.Session.GetString("UserDisplayName")) ? 
                    context.Session.GetString("UserEmail") : context.Session.GetString("UserDisplayName");
           }

           return string.Empty;
       }

       public static string GetLoginUserEmail(this HttpContext context) {
           if(context.IsUserLogin()){
               return 
                    context.Session.GetString("UserEmail");
           }

           return string.Empty;
       }

       public static void SetLoginUserProfile(this HttpContext context, string userDisplayName, string email){
            context.Session.SetString("UserDisplayName", userDisplayName);
            context.Session.SetString("UserEmail", email);
       }

        public static void ClearLoginUserProfile(this HttpContext context){
            context.Session.Clear();
       }
    }
}