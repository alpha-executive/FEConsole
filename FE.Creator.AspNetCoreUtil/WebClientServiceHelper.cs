using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FE.Creator.FEConsole.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Serialization;

namespace FE.Creator.AspNetCoreUtil
{
   public static class WebClientServiceHelper
    {
        private static void PrepareRequestDefaultHeader(this HttpClient httpClient, 
                System.Net.Http.Headers.HttpContentHeaders contentHeaders)
        {
            if(contentHeaders != null)
            {
                contentHeaders.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            }
        }

        private static async Task<T> ReadAsync<T>(this HttpResponseMessage response)
        {
            if(response != null &&
                response.IsSuccessStatusCode)
            {
                var responseStr = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseStr);
            }

            return default(T);
        }
        public static async Task<Dictionary<string,string>> GetSystemConfigurations(this HttpClient httpClient, string apiBaseUri, List<string> configKeys)
        {
            Dictionary<string, string> keyValues = null;
            string requestBody = JsonConvert.SerializeObject(configKeys);
            var stringContent = new StringContent(requestBody);
            
            httpClient.PrepareRequestDefaultHeader(stringContent.Headers);

            var response = await httpClient.PostAsync(string.Format("{0}/{1}", apiBaseUri, "api/SiteAdmin/GetAppConfig"),
                stringContent);

            if (response.IsSuccessStatusCode)
            {
                keyValues = await response.ReadAsync<Dictionary<string, string>>();
            }
           
            return keyValues;
        }

        public static async Task LogEvent(this HttpClient httpClient, string apiBaseUri, AppEventModel appEvent)
        {
            string requestBody = JsonConvert.SerializeObject(appEvent);
            var stringContent = new StringContent(requestBody);
            httpClient.PrepareRequestDefaultHeader(stringContent.Headers);

           var response =  await httpClient.PostAsync(string.Format("{0}/{1}", apiBaseUri, "api/SiteAdmin/LogEvent"),
                stringContent);
        }

        public static async Task<UserInfoResponse> GetLoginUserProfile(this HttpClient httpClient, string authServerUrl,HttpContext httpContext)
        {
            var token = await httpContext.GetUserAccessTokenAsync();
            var disco = await httpClient.GetDiscoveryDocumentAsync(authServerUrl);
            var response = await httpClient.GetUserInfoAsync(new UserInfoRequest
            {
                Address = disco.UserInfoEndpoint,
                Token = token
            });

            return response;
        }

        public static async Task<List<ProviderNotification>> GetProviderNotificationsAsync(this HttpClient httpClient, string apiBaseUri, string language)
        {
            List<ProviderNotification> notifications = null;
            httpClient.DefaultRequestHeaders.Add("req_language", language);

            var requestUrl = string.Format("{0}/{1}", apiBaseUri, "api/SiteAdmin/RecentSystemNotifications");
            var response = await httpClient.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                notifications = await response.ReadAsync<List<ProviderNotification>>();
            }

            return notifications;
        }


        public static async Task<List<AppEventModel>> LatestSystemEvent(this HttpClient httpClient, string apiBaseUri, int eventCount)
        {
            List<AppEventModel> appEvents = null;
           
            var requestUrl = string.Format("{0}/{1}/{2}", apiBaseUri, "api/SiteAdmin/LatestSystemEvent", eventCount);
            httpClient.PrepareRequestDefaultHeader(null);

            var response = await httpClient.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                appEvents = await response.ReadAsync<List<AppEventModel>>();
            }

            return appEvents;
        }
        
        public static async Task<string> UploadFile(this HttpClient httpClient, string apiBaseUri, IFormFile file)
        {
            string fileUrl = string.Empty;

            string requestUri = string.Format("{0}/{1}", apiBaseUri, "api/Files?forContent=true");
            httpClient.PrepareRequestDefaultHeader(null);
            
            using (var stream = file.OpenReadStream())
            {
                MultipartFormDataContent multiContent = new MultipartFormDataContent();
                var streamContent = new StreamContent(stream);
                multiContent.Add(streamContent, "file", file.FileName);
                var response = await httpClient.PostAsync(requestUri, multiContent);

                if (response.IsSuccessStatusCode)
                {

                    var uploadedFile = await response.ReadAsync<UploadFile>();
                    fileUrl = uploadedFile.url;
                }
            }

            return fileUrl;
        }


        public static async Task<object> ForwardRequest(this HttpClient httpClient, string apiBaseUri, string targetUrl, object requestContext)
        {
            string requestUri = string.Format("{0}/{1}", apiBaseUri, targetUrl);
            string content = JsonConvert.SerializeObject(requestContext);
            StringContent reqBody = new StringContent(content);
            httpClient.PrepareRequestDefaultHeader(reqBody.Headers);

            var response = await httpClient.PostAsync(requestUri, reqBody);

            if (response.IsSuccessStatusCode)
            {
                var jString = await response.Content.ReadAsStringAsync();
                var returnObj = JsonConvert.DeserializeObject(jString);

                return returnObj;
            }

            return null;
        }

        private static bool IsDownloadableFile(System.Net.Http.Headers.ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null
                    && !string.IsNullOrEmpty(contentDisposition.FileName);
        }
        public static async Task<FileResult> DownloadFile(this HttpClient httpClient, string apiBaseUri, string fwdUrl)
        {
            string requestUri = string.Format("{0}{1}", apiBaseUri, fwdUrl);
            var response = await httpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode
                && IsDownloadableFile(response.Content.Headers.ContentDisposition))
            {
               var stream = await response.Content.ReadAsStreamAsync();
               var fileResult = new FileStreamResult(stream,
                   new MediaTypeHeaderValue(response.Content.Headers.ContentType.MediaType));

               fileResult.FileDownloadName = System.Web.HttpUtility.UrlDecode(response.Content.Headers.ContentDisposition.FileName);
                
               return fileResult;
            }

            return null;
        }

        public static async Task<FileResult> DownloadSharedObjectFile(this HttpClient httpClient, 
          int objectId,
          string apiBaseUri,
          string filePropertyName,
          string shareFieldName,
          bool thumbinal,
          ILogger logger)
        {
            string requestUri = string.Format("{0}/{1}/{2}/{3}/{4}", 
                                                apiBaseUri,
                                                "api/SharedObjects/DownloadSharedObjectFile",
                                                objectId,
                                                filePropertyName,
                                                shareFieldName);
            if (thumbinal)
            {
                requestUri = string.Format("{0}?thumbinal=true", requestUri);
            }

            var svStr = await httpClient.GetSharedServiceObjectById(
                       apiBaseUri,
                       objectId,
                       shareFieldName,
                       new string[] { filePropertyName },
                       logger
                   );
            var article = JsonConvert.DeserializeObject<SimpleServiceObject>(svStr);
            var fileFiled = article.GetPropertyValue<FileFiledValue>(filePropertyName);

            var stream = await httpClient.GetStreamAsync(requestUri);

            var mediaType = ".png".Equals(fileFiled.fileExtension) ? "image/png" 
                                : "application/octet-stream";

            return new FileStreamResult(stream, new MediaTypeHeaderValue(mediaType))
            {
                FileDownloadName = fileFiled.fileName
            };
        }

        public static async Task<string> GetSharedServiceObjects(this HttpClient httpClient, 
            string apiBaseUri,
            string objDefName,
            string shareFieldName,
            string[] properties,
            ILogger logger,
            int page,
            int pageSize)
        {
            string requestUri = string.Format("{0}/{1}/{2}/{3}?page={4}&pageSize={5}", apiBaseUri, 
                                    "api/SharedObjects/GetSharedServiceObjects", 
                                    objDefName, 
                                    shareFieldName,
                                    page,
                                    pageSize);
            string content = JsonConvert.SerializeObject(properties);
            StringContent reqContent = new StringContent(content);
            httpClient.PrepareRequestDefaultHeader(reqContent.Headers);
            var response = await httpClient.PostAsync(requestUri, reqContent);

            if (response.IsSuccessStatusCode)
            {
                string jobjectStr = await response.Content.ReadAsStringAsync();
                return jobjectStr;
            }

            return null;
        }


        public static async Task<string> GetSharedServiceObjectById(this HttpClient httpClient,
            string apiBaseUri,
            int objectId,
            string shareFieldName,
            string[] properties,
            ILogger logger)
        {
            string requestUri = string.Format("{0}/{1}/{2}/{3}",
                                    apiBaseUri,
                                    "api/SharedObjects/GetSharedServiceObjectById",
                                    objectId,
                                    shareFieldName);
            logger.LogDebug("transfer the request to: " + requestUri);

            string content = JsonConvert.SerializeObject(properties);
            StringContent reqContent = new StringContent(content);
            httpClient.PrepareRequestDefaultHeader(reqContent.Headers);
            var response = await httpClient.PostAsync(requestUri, reqContent);

            if (response.IsSuccessStatusCode)
            {
                string jobjectStr = await response.Content.ReadAsStringAsync();
                logger.LogDebug(jobjectStr);
                return jobjectStr;
            }

            return null;
        }

        public static async Task<int> GetSharedServiceObjectsCount(this HttpClient httpClient,
           string apiBaseUri,
           string objDefName,
           string shareFieldName,
           ILogger logger)
        {
            string requestUri = string.Format("{0}/{1}/{2}", apiBaseUri,
                                    "api/SharedObjects/GetSharedServiceObjectsCount",
                                    objDefName);
            string content = JsonConvert.SerializeObject(shareFieldName);

            StringContent reqContent = new StringContent(content);
            httpClient.PrepareRequestDefaultHeader(reqContent.Headers);
            var response = await httpClient.PostAsync(requestUri, reqContent);

            if (response.IsSuccessStatusCode)
            {
                int count = await response.ReadAsync<int>();
                return count;
            }

            return 0;
        }

        public static async Task<string> GetSysConfiguredLanguage(this HttpClient httpClient, string apiBaseUri, ILogger logger)
        {
            string lang = string.Empty;
            try
            {
                var configs = await httpClient.GetSystemConfigurations(apiBaseUri,
                new List<string>() { "language" });

                lang = configs != null && configs.Count > 0 ?
                                    configs["language"] : string.Empty;
            }
            catch(Exception ex)
            {
                if(logger != null)
                {
                    logger.LogError(ex, "error when get language config from server.");
                }
            }
            
            return lang;
        }
    }
}
