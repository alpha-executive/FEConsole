using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using coreaspnet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
/* using Microsoft.AspNetCore.Identity; */
using System.Net.Http;
using System.Security.Claims;
using FE.Creator.PCenter.CutomProvider;
using FE.Creator.PCenter.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FE.Creator.PCenter {
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller {
        private readonly ApplicationPartManager _partManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        /* 
                private readonly SignInManager<IdentityUser> _signInManager;
                private readonly UserManager<IdentityUser> _userManager; */
        private readonly ILogger logger;
        public HomeController (ApplicationPartManager appPartManager,
            IConfiguration configuration,
            /*     SignInManager<IdentityUser> signInManager,
                UserManager<IdentityUser> userManager, */
            IHttpClientFactory httpClientFactory,
            ILogger<HomeController> logger) {
            this._partManager = appPartManager;
            this._configuration = configuration;
            /*             this._signInManager = signInManager;
                        this._userManager = userManager; */
            this.logger = logger;
            this._httpClientFactory = httpClientFactory;
            //shared resource should be injected here, or it was not available in the view. bug?
        }
        public IActionResult Index () {
            return View ();
        }

        public IActionResult Privacy () {
            return View ();
        }

        public RedirectResult PaypalPay () {
            var configSection = _configuration.GetSection ("SiteSettings:Payment");
            string paypalUrl = configSection.GetValue<string> ("paypalUrl");
            RedirectResult redirect = new RedirectResult (paypalUrl);
            return redirect;
        }

        public RedirectResult LivePreview () {
            var configSection = _configuration.GetSection ("SiteSettings:Products:FEConsole");
            string livePreviewUrl = configSection.GetValue<string> ("livePreviewUrl");
            RedirectResult redirectResult = new RedirectResult (livePreviewUrl);

            return redirectResult;
        }

        [Route ("/[controller]/[action]/{paymethod}")]
        public async Task<FileResult> PayImage (string paymethod) {
            string fileName = string.Empty;
            var configSection = _configuration.GetSection ("SiteSettings:Payment");
            if ("alipay".Equals (paymethod, StringComparison.InvariantCultureIgnoreCase)) {
                string alipayBRCode = configSection.GetValue<string> ("alipayBRCode");
                fileName = alipayBRCode;
                logger.LogDebug ($"alipayBRCode configured as: ${fileName}");
            }

            if ("wechatpay".Equals (paymethod, StringComparison.InvariantCultureIgnoreCase)) {
                string wechatBRCode = configSection.GetValue<string> ("wechatBRCode");
                fileName = wechatBRCode;
                logger.LogDebug ($"wechatBRCode configured as: ${fileName}");
            }

            FileResult result = new VirtualFileResult (fileName, "application/image");

            return await Task.FromResult (result);
        }

        [Route ("/[controller]/[action]/{producttype}")]
        public async Task<FileResult> Download (string producttype) {
            FileResult result = null;
            try {
                var configSection = _configuration.GetSection ("SiteSettings:Products:FEConsole");
                string downloadUrl = configSection.GetValue<string> ($"{producttype}DownloadUrl");
                string feconsoleVersion = configSection.GetValue<string> ("feconsoleVersion");
                UriBuilder builder = new UriBuilder (downloadUrl);

                WebClient client = new WebClient ();
                byte[] downloadData = await client.DownloadDataTaskAsync (builder.Uri);
                result = new FileContentResult (downloadData, "application/octet-stream");
                result.FileDownloadName = $"feconsole-{feconsoleVersion}-{producttype}.zip";
            } catch (Exception ex) {
                throw ex;
            }

            return result;
        }

        [ResponseCache (Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error () {
            return View (new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /*Uncomment it if need to authenticate external user with Asp.netcore identity.
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl=null)
        {
            string redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Home", new { returnUrl = returnUrl});
            var externalOptions = _signInManager.ConfigureExternalAuthenticationProperties(provider, 
                                    redirectUrl);

            return Challenge(externalOptions, provider);   
        }

        public async Task<IActionResult>  ExternalLoginCallback(string returnUrl) {

            var info = await _signInManager.GetExternalLoginInfoAsync();
            var redirectUrl = returnUrl ?? "~/";

            if(info != null) {
                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
               if(!result.Succeeded) {
                 
                 var emailCliam = info.Principal.FindFirst(ClaimTypes.Email);
                 var userNameCliam = info.Principal.FindFirst(ClaimTypes.Name);
                 if(userNameCliam == null)
                    userNameCliam = emailCliam;
                 
                 if(emailCliam != null){
                    var user = new IdentityUser { UserName = userNameCliam.Value, Email = emailCliam.Value};
                    await _userManager.CreateAsync(user);
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent:false);
                 }
               }
            }

            return  LocalRedirect(redirectUrl);
        } */

        public IActionResult Login (string returnUrl) {
            string redirectUrl = Url.Action (nameof (LoginCallback), "Home", new { returnUrl = returnUrl });
            var externalOptions = new AuthenticationProperties () {
                RedirectUri = redirectUrl
            };

            return Challenge (externalOptions);
        }

        private async Task<UserInfoResponse> GetLoginUserProfile () {
            var token = await HttpContext.GetUserAccessTokenAsync ();
            var client = this._httpClientFactory.CreateClient("client");
            // new HttpClient ();
            var authServerUrl = _configuration.GetSection ("Authentication:IdentityServer")
                .GetValue<string> ("Url");
            var disco = await client.GetDiscoveryDocumentAsync (authServerUrl);
            var response = await client.GetUserInfoAsync (new UserInfoRequest {
                Address = disco.UserInfoEndpoint,
                Token = token
            });

            return response;
        }

        public async Task<IActionResult> LoginCallback (string returnUrl) {
            string redirectUrl = string.IsNullOrEmpty (returnUrl) ? "~/" : returnUrl;
            var response = await GetLoginUserProfile ();
        var authServerUrl = _configuration.GetSection ("Authentication:IdentityServer")
                .GetValue<string> ("Url");
            //login failed, clear the current login session.
            string userName = response.TryGet ("name");
            string email = response.TryGet ("email");
            if (!string.IsNullOrEmpty (email)) {
                HttpContext.SetLoginUserProfile (userName, email);
                logger.LogDebug ($"User {userName} is login");
            } else {
                logger.LogWarning ("Failed to retrive login user's profile");
                return await Logout ();
            }

            return LocalRedirect (redirectUrl);
        }
        public async Task<bool> SendMessageToServer(string subject, string message){

            //var token = await HttpContext.GetUserAccessTokenAsync ();
            var client = this._httpClientFactory.CreateClient("client");
            var messageServerUrl = _configuration.GetSection ("SiteSettings:Products:FEConsole")
                                .GetValue<string>("feconsoleApiUrl");
            var sendData = new FormUrlEncodedContent(new[]
                            {
                                new KeyValuePair<string, string>("username", HttpContext.GetLoginUserDisplayName ()), 
                                new KeyValuePair<string, string>("email", HttpContext.GetLoginUserEmail ()),
                                new KeyValuePair<string, string>("subject", subject),
                                new KeyValuePair<string, string>("message", message)
                            });
            
            var response = await client.PostAsync(messageServerUrl, sendData);
            return response.IsSuccessStatusCode;
        } 
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage (ContactMessage formData) {
            //var applicationUser = await _userManager.GetUserAsync(User);
            if (HttpContext.IsUserLogin ()) {
                logger.LogDebug (HttpContext.GetLoginUserDisplayName () + "is sending message to server.");
                await SendMessageToServer(formData.Subject, formData.Message);
            } else {
                logger.LogError ("Not allowed to send message when a user is not login");
            }

            return Ok ();
        }

        public async Task<IActionResult> Logout () {
            /*  if(_signInManager.IsSignedIn(User)){
                 await _signInManager.SignOutAsync();
             } */
            await HttpContext.SignOutAsync ("Cookies");
            await HttpContext.SignOutAsync ("oidc");
            HttpContext.ClearLoginUserProfile ();
            //SignOut("Cookies", "oidc");
            return RedirectToAction ("Index");
        }
    }
}