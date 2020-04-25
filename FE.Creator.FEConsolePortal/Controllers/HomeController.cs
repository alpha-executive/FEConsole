using System;
using System.IO;
using System.Linq;
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

namespace FE.Creator.FEConsolePortal
{
    public class HomeController : Controller
    {
        private static readonly string DONATE_URL = "http://fetech.com/donate#";
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HomeController> _logger = null;
        public HomeController(IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<HomeController> logger)
        {
            this._configuration = configuration;
            this._httpClientFactory = httpClientFactory;
            this._logger = logger;
        }

        [HttpGet]
        public async Task<FileResult> GlobalJSVariableFile()
        {
            return await Task.Run<FileResult>(() =>
            {
                //portal do not need the base webapi.
                var globalJs = string.Format("var baseUrl = '{0}';", string.Empty);
                byte[] jsContent = UTF8Encoding.UTF8.GetBytes(globalJs);
                var jsFile = new FileContentResult(jsContent, new MediaTypeHeaderValue("text/javascript"));

                return jsFile;
            });
        }

        protected async Task<string> getAppSettingsLang()
        {
            var client = _httpClientFactory.CreateClient("client");
            /*var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/
            string lang = await client.GetSysConfiguredLanguage(HttpContext.WebApiBaseUrl(), this._logger);
            
            return lang;
        }

        // GET: Portal/PortalHome
        public async Task<ActionResult> Index()
        {
            string flxSliderView = await PortalFlexSlider();
            return View(nameof(Index), flxSliderView);
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
        public async Task<FileResult> DownloadSharedDocument(string Id)
        {
            var client = _httpClientFactory.CreateClient("client");
            /*var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/

            return await client.DownloadSharedObjectFile(Id,
                HttpContext.WebApiBaseUrl(),
                "documentFile",
                "documentSharedLevel",
                false);
        }

        /// <summary>
        /// For thumbinal download : /api/SharedObjects/DownloadSharedBook/{objectid}?thumbinal=true
        /// For File Download : /api/SharedObjects/DownloadSharedBook/{objectid}
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="thumbinal"></param>
        /// <returns></returns>
        [HttpGet("/[controller]/[action]/{Id}")]
        public async Task<FileResult> DownloadSharedBook(string Id, bool thumbinal = false)
        {
            var client = _httpClientFactory.CreateClient("client");

            return await client.DownloadSharedObjectFile(Id,
                HttpContext.WebApiBaseUrl(),
                "bookFile",
                "bookSharedLevel",
                thumbinal);
        }

        /// <summary>
        /// For thumbinal download : /api/SharedObjects/DownloadSharedImage/{objectid}?thumbinal=true
        /// For File Download : /api/SharedObjects/DownloadSharedImage/{objectid}
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="thumbinal"></param>
        /// <returns></returns>
        [HttpGet("/[controller]/[action]/{Id}")]
        public async Task<FileResult> DownloadSharedImage(string Id, bool thumbinal = false)
        {
            var client = _httpClientFactory.CreateClient("client");
            /*var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/

            return await client.DownloadSharedObjectFile(Id,
                HttpContext.WebApiBaseUrl(),
                "imageFile",
                "imageSharedLevel",
                thumbinal);
        }

        /// <summary>
        ///  For File Download : /api/SharedObjects/DownloadArticleImage/{objectid}
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("/[controller]/[action]/{Id}")]
        public async Task<FileResult> DownloadArticleImage(string Id)
        {
            var client = _httpClientFactory.CreateClient("client");
            /*var token = await HttpContext.GetClientAccessTokenAsync();
            client.SetBearerToken(token);*/

            return await client.DownloadSharedObjectFile(Id,
                HttpContext.WebApiBaseUrl(),
                "articleImage",
                "articleSharedLevel",
                 false);
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

        public async Task<string> PortalFlexSlider()
        {
            string lang = await getAppSettingsLang();

            if (!string.IsNullOrEmpty(lang))
            {
                if ("zh-CN".Equals(lang, StringComparison.InvariantCultureIgnoreCase))
                {
                    return "PortalFlexSlider_ZH_CN";
                }
            }
            else
            {
                //if language is not set in appsettings, apply chinese language if it's in chinese environment.
                if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.Equals("zh-CN", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "PortalFlexSlider_ZH_CN";
                }
            }

            return "PortalFlexSlider";
        }
    }
}