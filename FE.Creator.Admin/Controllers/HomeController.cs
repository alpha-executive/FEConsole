using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FE.Creator.FEConsole.Shared.Models;
using Microsoft.Extensions.FileProviders;
using FE.Creator.AspNetCoreUtil;
using System.IO;
using System.Text;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using IdentityModel.Client;
using System.Web;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Localization;

namespace FE.Creator.Admin.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<AppLang> _sharedResource;
        private readonly IFileProvider _fileProvider;
        ILogger<HomeController> logger = null;
        public HomeController(IHttpClientFactory httpClientFactory,
            IFileProvider fileProvider,
            IStringLocalizer<AppLang> sharedResource,
            ILogger<HomeController> logger,
            IServiceProvider serviceProvider):base(serviceProvider) {
            this._httpClientFactory = httpClientFactory;
            this._sharedResource = sharedResource;
            this._fileProvider = fileProvider;
            this.logger = logger;
        }
        public ActionResult Index()
        {
            logger.LogDebug(string.Format("{0} access the home page", HttpContext.GetLoginUser()));
            return View();
        }
        public async Task<ActionResult> Error()
        {
            var exceptionHandlerPathFeature =
                            HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (!string.IsNullOrEmpty(exceptionHandlerPathFeature?.Error.Message))
            {
                logger.LogError(exceptionHandlerPathFeature?.Error.Message);
                AppEventModel errEvent = new AppEventModel();
                errEvent.EventTitle = _sharedResource["ERROR_SERVER_ERROR"];
                errEvent.EventDetails = _sharedResource["ERROR_SERVER_ERROR_BODY", exceptionHandlerPathFeature?.Error.Message];
                errEvent.EventOwner = HttpContext.GetLoginUser();
                errEvent.EventLevel = AppEventModel.EnumEventLevel.Error;

                var client = _httpClientFactory.CreateClient("client");
                var baseUrl = HttpContext.WebApiBaseUrl();

                await client.LogEvent(baseUrl, errEvent);
            }

            return View();
        }

        public ActionResult About()
        {
            logger.LogDebug(string.Format("{0} access the About page", User.Identity.Name));
            return View("~/Views/AngularView/Server/About/About.cshtml");
        }

        public ActionResult Help()
        {
            logger.LogDebug(string.Format("{0} access the Help page", User.Identity.Name));
            string chineseHelp = "~/Views/AngularView/Server/Help/Help_ZH_CN.cshtml";
            string lang = HttpContext.GetCurrentCulture();
            //await GetSysConfiguredLanguage();

            if (!string.IsNullOrEmpty(lang))
            {
                if ("zh-CN".Equals(lang, StringComparison.InvariantCultureIgnoreCase)
                    || "zh".Equals(lang, StringComparison.InvariantCultureIgnoreCase))
                {
                    return View(chineseHelp);
                }
            }
            else
            {
                //if language is not set in appsettings, apply chinese language if it's in chinese environment.
                var threadLocale = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                if (threadLocale.Equals("zh-CN", StringComparison.InvariantCultureIgnoreCase)
                    || threadLocale.Equals("zh", StringComparison.InvariantCultureIgnoreCase))
                {
                    return View(chineseHelp);
                }
            }

            return View("~/Views/AngularView/Server/Help/Help.cshtml");
        }

      
        private async Task<string> GetSysConfiguredLanguage()
        {
            var client = _httpClientFactory.CreateClient("client");
            var baseUrl = HttpContext.WebApiBaseUrl();
            
            var lang = await client.GetSysConfiguredLanguage(baseUrl, logger);
             
            return lang;
        }

        protected FileResult getLanguageJSFilePath(string lang)
        {
            string enUsPath = _fileProvider.GetFileInfo(Path.Combine("js", "lang", "applang.en_us.js")).PhysicalPath;
            string zhCNPath = _fileProvider.GetFileInfo(Path.Combine("js", "lang", "applang.zh_cn.js")).PhysicalPath;

            if (!string.IsNullOrEmpty(lang))
            {
                if ("zh-CN".Equals(lang, StringComparison.InvariantCultureIgnoreCase)
                    ||"zh".Equals(lang, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new PhysicalFileResult(zhCNPath, new MediaTypeHeaderValue("text/javascript"));
                }
            }
            else
            {
                //if language is not set in appsettings, apply chinese language if it's in chinese environment.
                var threadLocale = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                if (threadLocale.Equals("zh-CN", StringComparison.InvariantCultureIgnoreCase)
                    || threadLocale.Equals("zh", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new PhysicalFileResult(zhCNPath, new MediaTypeHeaderValue("text/javascript"));
                }
            }

            return new PhysicalFileResult(enUsPath, new MediaTypeHeaderValue("text/javascript"));
        }

        [HttpGet]
        public ActionResult LocalizationJS(string locale)
        {
            var fileResult = getLanguageJSFilePath(locale);

            return fileResult;
        }

        [HttpGet]
        public async Task<FileResult> GlobalJSVariableFile()
        {
           return await Task.Run<FileResult>(() =>
            {
                var globalJs = string.Format("var baseUrl = '{0}';", HttpContext.WebApiBaseUrl());
                byte[] jsContent = UTF8Encoding.UTF8.GetBytes(globalJs);
                var jsFile = new FileContentResult(jsContent, new MediaTypeHeaderValue("text/javascript"));

                return jsFile;
            });
        }

        [HttpGet("/[controller]/[action]/{url}")]
        public async Task<FileResult> FwdDownload(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new FileNotFoundException();

            var client = _httpClientFactory.CreateClient("client");
            var token = await HttpContext.GetUserAccessTokenAsync();
            client.SetBearerToken(token);
            var fwdUrl = HttpUtility.UrlDecode(url);
            var fileResult = await client.DownloadFile(HttpContext.WebApiBaseUrl(), fwdUrl);

            return fileResult;
        }

        private async Task<List<ProviderNotification>> getProviderNotificationData()
        {
            logger.LogDebug("Start getProviderNotificationData");
            List<ProviderNotification> notifications = null;
           
            var client = _httpClientFactory.CreateClient("client");
            var baseUrl = HttpContext.WebApiBaseUrl();
           
            var configs =  await client.GetSystemConfigurations(baseUrl, 
                new List<string>() { "language", "pullMessageFromPublisher", "pullMessagePublisherUrl" });

            if (configs != null && configs.Count > 0)
            {
                string lang = configs["language"];
                bool isPullMessageFromPubliser = "1".Equals(configs["pullMessageFromPublisher"])
                            || "true".Equals(configs["pullMessageFromPublisher"], StringComparison.InvariantCultureIgnoreCase);
                logger.LogDebug("isPullMessageFromPubliser = " + isPullMessageFromPubliser);

                if (isPullMessageFromPubliser)
                {
                    string publisherUrl = configs["pullMessagePublisherUrl"];
                    logger.LogDebug("publisherUrl = " + publisherUrl);
                    notifications = await client.GetProviderNotificationsAsync(baseUrl, lang);
                }
            }

            return notifications;
        }

        [HttpGet]
        public async Task<ActionResult<String>> GetWebApiAccessToken()
        {
            string token = await HttpContext.GetUserAccessTokenAsync();
            return Json(new { token = token });
        }

        [HttpGet()]
        public async Task<ActionResult> ProviderNotification()
        {
            List<ProviderNotification> notifies = await getProviderNotificationData();
            if(notifies == null 
                || notifies.Count == 0)
            {
                logger.LogDebug("Not get any notification from provider site, will use the default one");
                notifies = new List<ProviderNotification>();
                var notifyData = new ProviderNotification
                {
                    NotifyDesc = _sharedResource["INDEX_NOTIFY_DEFAULT_MSG"].Value,
                    ImageSrc = Url.Content("~/lib/adminlte-2.3.6/dist/img/alarm-50.png"),
                    ActionUrl = "#",
                    Notifier = _sharedResource["INDEX_NOTIFY_DEFAULT_PROVIDER"].Value,
                    EventTime = DateTime.Now.ToShortDateString()
                };

                notifies.Add(notifyData);
            }

            return Json(notifies);
        }

        [HttpPost]
        public async Task<ActionResult<List<AppEventModel>>> LatestSystemEvent(int count)
        {
            HttpClient client = _httpClientFactory.CreateClient("client");
            var baseUrl = HttpContext.WebApiBaseUrl();
            List<AppEventModel> latestEvents = await client.LatestSystemEvent(baseUrl, count);

            return Json(latestEvents);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public  ActionResult LogOff()
        {
           return SignOut("Cookies", "oidc");
        }
    }
}