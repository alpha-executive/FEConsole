using FE.Creator.Admin.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FE.Creator.AspNetCoreUtil;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace FE.Creator.Admin.Controllers.ckEditorUploadController
{
    public class FileUploadController : Controller
    {
        
        private readonly ILogger<FileUploadController> logger = null;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string baseUrl = null;
        public FileUploadController(IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<FileUploadController> logger)
        {
            this._httpClientFactory = httpClientFactory;
            this.baseUrl = configuration["SiteSettings:FEconsoleApiUrl"];
            this.logger = logger;
        }

        [HttpPost]
        [RequestSizeLimit(1024 * 1024 * 100)]
        // post: Upload File
        public async Task<ActionResult> CKEditorUpload(IFormFile upload)
        {
            //string rootPath = System.IO.Path.Combine(System.Web.HttpRuntime.AppDomainAppPath, "App_Data");
            //IFileStorageService storageService = new LocalFileSystemStorage(rootPath);
            logger.LogDebug("Start CKEditorUpload");
            CKUploadModels model = new CKUploadModels();
            model.FunctionNumber = HttpContext.Request.Query["CKEditorFuncNum"];
            
            if(upload != null)
            {
                var client = _httpClientFactory.CreateClient("client");
                var token = await HttpContext.GetUserAccessTokenAsync();
                client.SetBearerToken(token);
                string url = await client.UploadFile(this.baseUrl, upload);
                var encodeUrl = string.Format("/home/fwddownload/{0}",  HttpUtility.UrlEncode(url));
                model.Url = encodeUrl;
            }

            logger.LogDebug("download url: " + model.Url);
            logger.LogDebug("End CKEditorUpload");
            return View(model);
        }

        [HttpPost]
        [RequestSizeLimit(1024 * 1024 * 100)]
        public async Task<JsonResult> CKImageUpload(IFormFile upload)
        {
            if (upload != null)
            {
                var client = _httpClientFactory.CreateClient("client");
                var token = await HttpContext.GetUserAccessTokenAsync();
                client.SetBearerToken(token);
                string url = await client.UploadFile(this.baseUrl, upload);
                var encodeUrl = string.Format("/home/fwddownload/{0}", HttpUtility.UrlEncode(url));
                return Json(new { uploaded = 1, fileName = upload.FileName, url = encodeUrl });
            }

            return Json(new { uploaded = 0 });
        }
    }
}