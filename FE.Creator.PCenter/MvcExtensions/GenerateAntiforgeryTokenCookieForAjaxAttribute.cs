
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FE.Creator.PCenter.MvcExtensions
{
    public class GenerateAntiforgeryTokenCookieForAjaxAttribute: ActionFilterAttribute
    {
        private static readonly string XSRF_TOKEN = "XSRF-TOKEN";
        public override void OnActionExecuted(ActionExecutedContext context){
            var antiforgery = context
                                    .HttpContext
                                        .RequestServices
                                            .GetService(typeof(IAntiforgery)) as IAntiforgery;

            var tokens =antiforgery.GetAndStoreTokens(context.HttpContext);
            
            context.HttpContext.Response.Cookies.Append(
                XSRF_TOKEN,
                tokens.RequestToken,
                new Microsoft.AspNetCore.Http.CookieOptions() {HttpOnly = false}
            );
        }
    }
}