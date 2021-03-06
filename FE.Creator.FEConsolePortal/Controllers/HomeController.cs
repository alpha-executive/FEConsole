﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using FE.Creator.FEConsolePortal.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using FE.Creator.AspNetCoreUtil;
using System.Text;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Diagnostics;
using FE.Creator.FEConsole.Shared.Models;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace FE.Creator.FEConsolePortal
{
    public class HomeController : Controller
    {
        private static readonly string DONATE_URL = "http://fetech.com/donate#";
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HomeController> _logger = null;
        IStringLocalizer<ConsolePortal> _localResource = null;
        public HomeController(IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IStringLocalizer<ConsolePortal> localResource,
            ILogger<HomeController> logger)
        {
            this._configuration = configuration;
            this._httpClientFactory = httpClientFactory;
            this._logger = logger;
            this._localResource = localResource;
        }

        [HttpGet]
        public async Task<FileResult> GlobalJSVariableFile()
        {
            return await Task.Run<FileResult>(() =>
            {
                //portal do not need the base webapi baseUrl
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(string.Format("var baseUrl = '{0}';", string.Empty));
                stringBuilder.AppendLine(string.Format("var bookListPageSize = {0};", HttpContext.GetConfigValue<int>("SiteSettings:DisplaySetting:BookListPageSize")));
                stringBuilder.AppendLine(string.Format("var fileListPageSize = {0};", HttpContext.GetConfigValue<int>("SiteSettings:DisplaySetting:FileListPageSize")));
                stringBuilder.AppendLine(string.Format("var articleListPageSize = {0};", HttpContext.GetConfigValue<int>("SiteSettings:DisplaySetting:ArticleListPageSize")));
                stringBuilder.AppendLine(string.Format("var imageListPageSize = {0};", HttpContext.GetConfigValue<int>("SiteSettings:DisplaySetting:ImageListPageSize")));

                byte[] jsContent = UTF8Encoding.UTF8.GetBytes(stringBuilder.ToString());
                var jsFile = new FileContentResult(jsContent, new MediaTypeHeaderValue("text/javascript"));

                return jsFile;
            });
        }

        protected string getAppSettingsLang()
        {
            //var client = _httpClientFactory.CreateClient("client");
            ///*var token = await HttpContext.GetClientAccessTokenAsync();
            //client.SetBearerToken(token);*/
            //string lang = await client.GetSysConfiguredLanguage(HttpContext.WebApiBaseUrl(), this._logger);
            var lang = HttpContext.GetCurrentCulture();

            return lang;
        }

        // GET: Portal/PortalHome
        public ActionResult Index()
        {
            string flxSliderView = PortalFlexSlider();
            return View(nameof(Index), flxSliderView);
        }


        public async Task<ActionResult> Error()
        {
            var exceptionHandlerPathFeature =
                            HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (!string.IsNullOrEmpty(exceptionHandlerPathFeature?.Error.Message))
            {
                _logger.LogError(exceptionHandlerPathFeature?.Error.Message);
                AppEventModel errEvent = new AppEventModel();
                errEvent.EventTitle = _localResource["ERROR_PORTAL_ERROR"];
                errEvent.EventDetails = _localResource["ERROR_SERVER_ERROR_BODY", exceptionHandlerPathFeature?.Error.Message];
                errEvent.EventOwner = HttpContext.GetConfigValue<string>("SiteSettings:AdminUser");
                errEvent.EventLevel = AppEventModel.EnumEventLevel.Error;
                errEvent.EventSource = AppEventModel.EventSourceEnum.Portal;

                var client = _httpClientFactory.CreateClient("client");
                var baseUrl = HttpContext.WebApiBaseUrl();

                await client.LogEvent(baseUrl, errEvent);
            }

            return View();
        }


        public PartialViewResult AngularLightBoxTemplate()
        {
            return PartialView();
        }


        /// <summary>
        /// For File Download : /api/SharedObjects/DownloadSharedBook/{objectid}
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("/[controller]/[action]/{Id}")]
        public async Task<FileResult> DownloadSharedDocument(int Id)
        {
            var client = _httpClientFactory.CreateClient("client");
            /*var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/

            return await client.DownloadSharedObjectFile(Id,
                HttpContext.WebApiBaseUrl(),
                "documentFile",
                "documentSharedLevel",
                false,
                this._logger);
        }

        /// <summary>
        /// For thumbinal download : /api/SharedObjects/DownloadSharedBook/{objectid}?thumbinal=true
        /// For File Download : /api/SharedObjects/DownloadSharedBook/{objectid}
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="thumbinal"></param>
        /// <returns></returns>
        [HttpGet("/[controller]/[action]/{Id}")]
        public async Task<FileResult> DownloadSharedBook(int Id, bool thumbinal = false)
        {
            var client = _httpClientFactory.CreateClient("client");

            return await client.DownloadSharedObjectFile(Id,
                HttpContext.WebApiBaseUrl(),
                "bookFile",
                "bookSharedLevel",
                thumbinal,
                this._logger);
        }

        /// <summary>
        /// For thumbinal download : /api/SharedObjects/DownloadSharedImage/{objectid}?thumbinal=true
        /// For File Download : /api/SharedObjects/DownloadSharedImage/{objectid}
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="thumbinal"></param>
        /// <returns></returns>
        [HttpGet("/[controller]/[action]/{Id}")]
        public async Task<FileResult> DownloadSharedImage(int Id, bool thumbinal = false)
        {
            var client = _httpClientFactory.CreateClient("client");
            /*var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/

            return await client.DownloadSharedObjectFile(Id,
                HttpContext.WebApiBaseUrl(),
                "imageFile",
                "imageSharedLevel",
                thumbinal,
                this._logger);
        }

        /// <summary>
        ///  For File Download : /api/SharedObjects/DownloadArticleImage/{objectid}
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("/[controller]/[action]/{Id}")]
        public async Task<FileResult> DownloadArticleImage(int Id)
        {
            var client = _httpClientFactory.CreateClient("client");
            /*var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/

            return await client.DownloadSharedObjectFile(Id,
                HttpContext.WebApiBaseUrl(),
                "articleImage",
                "articleSharedLevel",
                 false,
                 this._logger);
        }

        /// <summary>
        /// /api/custom/SharedObjects/SharedArticles
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [Route("/[controller]/[action]")]
        [HttpGet]
        public async Task<ActionResult<string>> SharedArticles(int page = 1, int pagesize = int.MaxValue)
        {
            int currPage = page > 0 ? page : 1;
            int currPageSize = pagesize > 0 ? pagesize : int.MaxValue;

            var client = _httpClientFactory.CreateClient("client");
            //var token = await HttpContext.GetClientAccessTokenAsync();
            //client.SetBearerToken(token);
            var sharedObjects = await client.GetSharedServiceObjects(
                    HttpContext.WebApiBaseUrl(),
                    "Article",
                    "articleSharedLevel",
                    new string[] {
                        "articleDesc",
                        "isOriginal",
                        "articleImage",
                        "articleGroup",
                        "articleSharedLevel"
                    },
                   this._logger,
                   currPage,
                   currPageSize
                );
            
            return Json(sharedObjects);
        }

        /// <summary>
        /// /api/custom/SharedObjects/SharedImages
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [Route("/[controller]/[action]")]
        [HttpGet]
        public async Task<ActionResult<string>> SharedImages(int page = 1, int pagesize = int.MaxValue)
        {
            int currPage = page > 0 ? page : 1;
            int currPageSize = pagesize > 0 ? pagesize : int.MaxValue;

            var client = _httpClientFactory.CreateClient("client");
            /*var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/

            var sharedObjects = await client.GetSharedServiceObjects(
                    HttpContext.WebApiBaseUrl(),
                    "Photos",
                    "imageSharedLevel",
                    new string[] {
                         "imageFile",
                        "imageDesc",
                        "imageCategory",
                        "imageSharedLevel"
                    },
                   this._logger,
                   currPage,
                   currPageSize
                );

            return Json(sharedObjects);
        }

        /// <summary>
        /// /api/custom/SharedObjects/SharedBooks
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [Route("/[controller]/[action]")]
        [HttpGet]
        public async Task<ActionResult<string>> SharedBooks(int page = 1, int pagesize = int.MaxValue)
        {
            int currPage = page > 0 ? page : 1;
            int currPageSize = pagesize > 0 ? pagesize : int.MaxValue;

            var client = _httpClientFactory.CreateClient("client");
            /*var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/

            var sharedObjects = await client.GetSharedServiceObjects(
                    HttpContext.WebApiBaseUrl(),
                    "Books",
                    "bookSharedLevel",
                    new string[] {
                        "bookFile",
                        "bookDesc",
                        "bookAuthor",
                        "bookVersion",
                        "bookSharedLevel",
                        "bookCategory",
                        "bookISBN"
                    },
                   this._logger,
                   currPage,
                   currPageSize
                );

            return Json(sharedObjects);
        }


        /// <summary>
        /// /Home/SharedDocuments
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [Route("/[controller]/[action]")]
        [HttpGet]
        public async Task<ActionResult<string>> SharedDocuments(int page = 1, int pagesize = int.MaxValue)
        {
            int currPage = page > 0 ? page : 1;
            int currPageSize = pagesize > 0 ? pagesize : int.MaxValue;
            var client = _httpClientFactory.CreateClient("client");
            /*var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/

            var sharedObjects = await client.GetSharedServiceObjects(
                    HttpContext.WebApiBaseUrl(),
                    "Documents",
                    "documentSharedLevel",
                    new string[] {
                        "documentFile",
                        "documentSharedLevel"
                    },
                   this._logger,
                   currPage,
                   currPageSize
                );

            return Json(sharedObjects);
        }

        [Route("/[controller]/[action]")]
        [HttpGet]
        public async Task<int> GetSharedImageCount()
        {
            var client = _httpClientFactory.CreateClient("client");
            var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);

            var count = await client.GetSharedServiceObjectsCount(
                 HttpContext.WebApiBaseUrl(),
                 "Photos",
                 "imageSharedLevel",
                 this._logger
                );

            return count;
        }

        [Route("/[controller]/[action]")]
        [HttpGet]
        public async Task<int> GetSharedBookCount()
        {
            var client = _httpClientFactory.CreateClient("client");
            /*var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/

            var count = await client.GetSharedServiceObjectsCount(
                 HttpContext.WebApiBaseUrl(),
                 "Books",
                 "bookSharedLevel",
                 this._logger
                );

            return count;
        }

        [Route("/[controller]/[action]")]
        [HttpGet]
        public async Task<int> GetSharedArticleCount()
        {
            var client = _httpClientFactory.CreateClient("client");
            /* var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/

            var count = await client.GetSharedServiceObjectsCount(
                 HttpContext.WebApiBaseUrl(),
                 "Article",
                 "articleSharedLevel",
                 this._logger
                );

            return count;
        }

        [Route("/[controller]/[action]")]
        [HttpGet]
        public async Task<int> GetSharedDocumentCount()
        {
            var client = _httpClientFactory.CreateClient("client");
            /*var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/

            var count = await client.GetSharedServiceObjectsCount(
                 HttpContext.WebApiBaseUrl(),
                 "Documents",
                 "documentSharedLevel",
                 this._logger
                );

            return count;
        }


        [HttpGet("/[controller]/[action]/{url}")]
        public async Task<FileResult> FwdDownload(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new FileNotFoundException();

            var client = _httpClientFactory.CreateClient("client");
            
            var fwdUrl = HttpUtility.UrlDecode(url);
            var fileResult = await client.DownloadFile(HttpContext.WebApiBaseUrl(), fwdUrl);

            return fileResult;
        }

        [Route("/[controller]/[action]/{objectId}")]
        [HttpGet]
        public async Task<ActionResult> ViewArticleContent(int objectId)
        {
            PostViewModel viewModel = new PostViewModel(HttpContext);
            var client = _httpClientFactory.CreateClient("client");
            var svStr = await client.GetSharedServiceObjectById(
                        HttpContext.WebApiBaseUrl(),
                        objectId,
                        "articleSharedLevel",
                        new string[] {
                            "articleDesc",
                            "articleContent",
                            "isOriginal",
                            "articleImage",
                            "articleGroup",
                            "articleSharedLevel" },
                        this._logger
                    );

            var article = JsonConvert.DeserializeObject<SimpleServiceObject>(svStr);
            long sharedLevel = article.GetPropertyValue<long>("articleSharedLevel");

            if (sharedLevel == 1)
            {
                viewModel.ObjectId = article.objectID;
                viewModel.PostTitle = article.objectName;
                viewModel.PostDesc = article.GetPropertyValue<string>("articleDesc");
                viewModel.PostContent = article.GetPropertyValue<string>("articleContent");
                viewModel.IsOriginal = article.GetPropertyValue<long>("isOriginal") > 0;
                viewModel.Updated = article.updated;
                viewModel.Author = article.updatedBy;
            }

            return View(viewModel);
        }

        public ActionResult Donate()
        {
            string donateUrl = _configuration["SiteSettings:DonateUrl"];
            donateUrl = string.IsNullOrEmpty(donateUrl) ? DONATE_URL : donateUrl;
            RedirectResult result = new RedirectResult(donateUrl);

            return result;
        }

        public string PortalFlexSlider()
        {
            string lang = getAppSettingsLang();

            if (!string.IsNullOrEmpty(lang))
            {
                if ("zh-CN".Equals(lang, StringComparison.InvariantCultureIgnoreCase)
                    || "zh".Equals(lang, StringComparison.InvariantCultureIgnoreCase))
                {
                    return "PortalFlexSlider_ZH_CN";
                }
            }
            else
            {
                //if language is not set in appsettings, apply chinese language if it's in chinese environment.
                var threadCulture = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                if (threadCulture.Equals("zh-CN", StringComparison.InvariantCultureIgnoreCase)
                    || threadCulture.Equals("zh", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "PortalFlexSlider_ZH_CN";
                }
            }

            return "PortalFlexSlider";
        }
    }
}