using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FE.Creator.FEConsole.Shared.Models;
using FE.Creator.FEConsole.Shared.Services.Cryptography;
using FE.Creator.ObjectRepository;
using FE.Creator.ObjectRepository.EntityModels;
using FE.Creator.ObjectRepository.ServiceModels;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FE.Creator.FEConsoleAPI
{
    class TokenUserInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public string UserFriendlyName { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class FEAPIBaseController : ControllerBase
    {
        private readonly IMemoryCache _cache = null;
        private readonly ILogger _logger = null;
        private readonly IServiceProvider _provider = null;
        private readonly string _authServerUrl = null;
        private readonly bool   _requireHttps = false;
        private readonly ISHAService _shaService = null;
        private readonly int _cacheSize = 0;
        private readonly int _cacheExpired = 1;
        private static AutoResetEvent _cacheWriteLock = new AutoResetEvent(true);

        protected FEAPIBaseController(IServiceProvider provider)
        {
            this._provider = provider;
            this._cache = this._provider.GetRequiredService<IMemoryCache>();
            this._logger = this._provider.GetRequiredService<ILogger<FEAPIBaseController>>();
            var Configuration = this._provider.GetRequiredService<IConfiguration>();
            this._authServerUrl = Configuration.GetSection("Authentication:IdentityServer")
                        .GetValue<string>("Url");
            this._shaService = this._provider.GetRequiredService<ISHAService>();
            this._cacheSize = Configuration.GetValue<int>("SiteSettings:Cache:CacheSize");
            this._cacheExpired = Configuration.GetValue<int>("SiteSettings:Cache:AbsoluteExpiration");
            this._requireHttps = Configuration.GetSection("Authentication:IdentityServer")
                                               .GetValue<bool>("RequireHttpsMetadata");
        }


        async Task<TokenUserInfo> CreateClaimTask()
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("token is empty");
                return new TokenUserInfo();
            }

            byte[] tokenBytes = UTF8Encoding.UTF8.GetBytes(token);
            var tokenKey = _shaService.CalculateSHA256(tokenBytes);

            Task<UserInfoResponse> taskEntry = null;
            if (!_cache.TryGetValue<Task<UserInfoResponse>>(tokenKey, out taskEntry))
            {
                taskEntry = CacheTokenValidationTask(token, tokenKey);
            }
            else
            {
                _logger.LogDebug("Hit token cache, value set by the peer thread.");
            }
            var response = await taskEntry;

            var userInfo = new TokenUserInfo();
            userInfo.UserId = response?.TryGet(JwtClaimTypes.Subject)
                    ?? response?.TryGet(ClaimTypes.NameIdentifier);
            userInfo.UserName = response?.TryGet(JwtClaimTypes.Email)
                    ?? response?.TryGet(JwtClaimTypes.EmailVerified);
            userInfo.UserFriendlyName = response?.TryGet(JwtClaimTypes.Name)??
                                            response?.TryGet(JwtClaimTypes.FamilyName);

            if (string.IsNullOrEmpty(userInfo.UserFriendlyName))
            {
                userInfo.UserFriendlyName = userInfo.UserName;
            }

            if (string.IsNullOrEmpty(userInfo.UserId)
              || string.IsNullOrEmpty(userInfo.UserName))
            {
                //clean the task that failed to get the user token.
                _cache.Remove(tokenKey);
                _logger.LogWarning("failed to get the user cliam from the identity server response.");
            }

            return userInfo;
        }

        /// <summary>
        /// the legacy way to cache token and token retrive task
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        //async Task<TokenUserInfo> CreateClaimTask()
        //{
        //    var token = await HttpContext.GetTokenAsync("access_token");
        //    if (string.IsNullOrEmpty(token))
        //    {
        //        _logger.LogWarning("token is empty");
        //        return new TokenUserInfo();
        //    }

        //    byte[] tokenBytes = UTF8Encoding.UTF8.GetBytes(token);
        //    var tokenKey = _shaService.CalculateSHA256(tokenBytes);
        //    if (!_cache.TryGetValue(tokenKey, out TokenUserInfo responseEntry))
        //    {
        //        _logger.LogDebug("NOT hit cache for token: " + tokenKey);
        //        var cacheEntryOptions = new MemoryCacheEntryOptions()
        //                 // Set cache entry size by extension method.
        //                 .SetSize(_cacheSize)
        //                 // Keep in cache for this time, reset time if accessed.
        //                 .SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheExpired));

        //        var cacheWriteKey = tokenKey + "_cacheWrite";

        //        Task<UserInfoResponse> taskEntry = null;
        //        try
        //        {
        //            _cacheWriteLock.WaitOne();

        //            _logger.LogDebug("enter task cache lock-area");
        //            if (!_cache.TryGetValue<Task<UserInfoResponse>>(cacheWriteKey, out taskEntry))
        //            {
        //                _logger.LogDebug("NOT hit cache in the task cache lock-area: " + cacheWriteKey);
        //                taskEntry = _cache.Set(cacheWriteKey,
        //                                CreatTokenValidationTask(token),
        //                                cacheEntryOptions);
        //                _logger.LogDebug("set task cache in the task cache lock-area");
        //            }
        //            else
        //            {
        //                _logger.LogDebug("Hit cache in the task cache lock-area: " + cacheWriteKey);
        //            }
        //        }
        //        finally
        //        {
        //            _cacheWriteLock.Set();
        //        }

        //        var response = await taskEntry;
        //        //try last step to identify if it need to write the cache.
        //        if (response?.Claims.Count() > 0)
        //        {
        //            try
        //            {
        //                _cacheWriteLock.WaitOne();
        //                if (!_cache.TryGetValue(tokenKey, out responseEntry))
        //                {
        //                    responseEntry = new TokenUserInfo();
        //                    responseEntry.UserId = response?.TryGet(JwtClaimTypes.Subject)
        //                            ?? response?.TryGet(ClaimTypes.NameIdentifier);
        //                    responseEntry.UserName = response?.TryGet(JwtClaimTypes.Email)
        //                            ?? response?.TryGet(JwtClaimTypes.EmailVerified);

        //                    if (!string.IsNullOrEmpty(responseEntry.UserId)
        //                      && !string.IsNullOrEmpty(responseEntry.UserName))
        //                    {
        //                        _logger.LogDebug("set token cache entry: " + tokenKey);
        //                        // Save data in cache.
        //                        _cache.Set(tokenKey, responseEntry, cacheEntryOptions);
        //                    }
        //                    else
        //                    {
        //                        //clean the task that failed to get the user token.
        //                        _cache.Remove(cacheWriteKey);
        //                        _logger.LogWarning("failed to get the user cliam from the identity server response.");
        //                    }
        //                }
        //                else
        //                {
        //                    _logger.LogDebug("hit token cache, value was set by peer the thread.");
        //                }


        //            }
        //            finally
        //            {
        //                _cacheWriteLock.Set();
        //            }
        //        }

        //        //Another way to retrieve the User info from identity server with high performance.
        //        //AutoResetEvent accessWriterLocker = null;
        //        //try
        //        //{
        //        //    if (!_cacheWriteLock.IsWriteLockHeld)
        //        //    {
        //        //        _cacheWriteLock.EnterWriteLock();
        //        //    }

        //        //    _logger.LogDebug("enter write cache lock-area");
        //        //    if (!_cache.TryGetValue<AutoResetEvent>(cacheWriteKey, out accessWriterLocker))
        //        //    {
        //        //        _logger.LogDebug("NOT hit cache in the write cache lock-area: " + cacheWriteKey);
        //        //        accessWriterLocker = new AutoResetEvent(true);
        //        //        _cache.Set(cacheWriteKey, accessWriterLocker, cacheEntryOptions);
        //        //        _logger.LogDebug("write cache set in the lock-area");
        //        //    }
        //        //    else
        //        //    {
        //        //        _logger.LogDebug("Hit cache in the write cache lock-area: " + cacheWriteKey);
        //        //    }
        //        //}
        //        //finally
        //        //{
        //        //    if (_cacheWriteLock.IsWriteLockHeld)
        //        //    {
        //        //        _cacheWriteLock.ExitWriteLock();
        //        //    }
        //        //}


        //        //block the thread to validate the same token.
        //        //try
        //        //{
        //        //    accessWriterLocker.WaitOne();
        //        //    _logger.LogDebug("enter the write lock area for token validation.");
        //        //    if (!_cache.TryGetValue<TokenUserInfo>(tokenKey, out responseEntry))
        //        //    {
        //        //        _logger.LogDebug("NOT hit the cache in write lock area for cache key: " + tokenKey);
        //        //        var response = await CreatTokenValidationTask(token);

        //        //        //try last step to identify if it need to write the cache.
        //        //        if (response?.Claims.Count() > 0
        //        //            && !_cache.TryGetValue(tokenKey, out responseEntry))
        //        //        {
        //        //            responseEntry = new TokenUserInfo();
        //        //            responseEntry.UserId = response?.TryGet(JwtClaimTypes.Subject)
        //        //                    ?? response?.TryGet(ClaimTypes.NameIdentifier);
        //        //            responseEntry.UserName = response?.TryGet(JwtClaimTypes.Email)
        //        //                    ?? response?.TryGet(JwtClaimTypes.EmailVerified);

        //        //            if(!string.IsNullOrEmpty(responseEntry.UserId)
        //        //                && !string.IsNullOrEmpty(responseEntry.UserName))
        //        //            {
        //        //                _logger.LogDebug("cache entry set: " + tokenKey);
        //        //                // Save data in cache.
        //        //                _cache.Set(tokenKey, response, cacheEntryOptions);
        //        //            }
        //        //        }
        //        //    }
        //        //    else
        //        //    {
        //        //        _logger.LogDebug("hit the cache in write lock area for cache Key:" + tokenKey);
        //        //    }
        //        //}
        //        //finally
        //        //{
        //        //    //release the waiting thread.
        //        //    if(accessWriterLocker != null)
        //        //    {
        //        //        accessWriterLocker.Set();
        //        //    }
        //        //}

        //    }
        //    else
        //    {
        //        _logger.LogDebug("Hit cache for token: " + tokenKey);
        //    }

        //    return responseEntry;
        //}

        /// <summary>
        /// the legacy way to cache token and token retrive task
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokenKey"></param>
        /// <returns></returns>
        private Task<UserInfoResponse> CacheTokenValidationTask(string token, string tokenKey)
        {
            Task<UserInfoResponse> taskEntry = null;
            try
            {
                _cacheWriteLock.WaitOne();
                _logger.LogDebug("enter task cache lock-area");
                if (!_cache.TryGetValue<Task<UserInfoResponse>>(tokenKey, out taskEntry))
                {
                    _logger.LogDebug("NOT hit cache in the task cache lock-area: " + tokenKey);
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                         // Set cache entry size by extension method.
                         .SetSize(_cacheSize)
                         // Keep in cache for this time, reset time if accessed.
                         .SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheExpired));

                    taskEntry = _cache.Set(tokenKey,
                                    CreatTokenValidationTask(token),
                                    cacheEntryOptions);
                    _logger.LogDebug("set task cache in the task cache lock-area");
                }
                else
                {
                    _logger.LogDebug("Hit cache in the task cache lock-area: " + tokenKey);
                }
            }
            finally
            {
                _cacheWriteLock.Set();
            }

            return taskEntry;
        }
        private Task<UserInfoResponse> CreatTokenValidationTask(string token)
        {
            return Task.Run<UserInfoResponse>(async () =>
            {
                _logger.LogDebug("about to validate the token on remote identity server.");
                var client = new HttpClient();
                var discoRequest = new DiscoveryDocumentRequest(){
                   Address = this._authServerUrl,
                    Policy = new DiscoveryPolicy(){
                        RequireHttps = this._requireHttps
                    }
                };
                var disco = await client.GetDiscoveryDocumentAsync(discoRequest);

                client.SetBearerToken(token);
                var response = await client.GetUserInfoAsync(new UserInfoRequest
                {
                    Address = disco.UserInfoEndpoint,
                    Token = token
                });
                _logger.LogDebug("get validate resonse from identity server.");
                return response;
            });
        }

        /// <summary>
        /// The unique flag of the user, currently using the registered email.
        /// </summary>
        /// <returns></returns>
        internal async Task<string> GetLoginUser()
        {
            var userInfo = await CreateClaimTask();
            if (string.IsNullOrEmpty(userInfo?.UserName))
            {
                throw new ArgumentNullException("Can not get the user name.");
            }

            return userInfo.UserName;
        }


        internal async Task<string> GetUserFriendlyName()
        {
            var userInfo = await CreateClaimTask();
            if (string.IsNullOrEmpty(userInfo?.UserFriendlyName))
            {
                throw new ArgumentNullException("Can not get the user name.");
            }

            return userInfo.UserFriendlyName;
        }
        /// <summary>
        /// the user identify key.
        /// </summary>
        /// <returns></returns>
        internal async Task<string> GetLoginUserId()
        {
            var userInfo = await CreateClaimTask();

            if(string.IsNullOrEmpty(userInfo?.UserId))
            {
                throw new ArgumentNullException("Can not get the user id.");
            }    
            return userInfo.UserId;
        }

        protected async Task<int> LogAppEvent(IObjectService objectService, AppEventModel logEvent)
        {
            string owner = logEvent.EventSource == AppEventModel.EventSourceEnum.Portal ? "portal" 
                :  await GetLoginUser();
            ServiceObject svObject = new ServiceObject();
            svObject.ObjectName = "System Event";
            svObject.ObjectOwner = owner;
            svObject.OnlyUpdateProperties = false;
            svObject.UpdatedBy = owner;
            svObject.CreatedBy = owner;
            svObject.ObjectDefinitionId = objectService
                .GetObjectDefinitionByName("AppEvent")
                .ObjectDefinitionID;

            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "eventTitle",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.String,
                    Value = logEvent.EventTitle
                }
            });
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "eventDetails",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.String,
                    Value = logEvent.EventDetails
                }
            });

            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "eventDateTime",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.Datetime,
                    Value = DateTime.Now
                }
            });
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "eventLevel",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.Integer,
                    Value = (int)logEvent.EventLevel
                }
            });

            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "eventOwner",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.String,
                    Value = owner
                }
            });

            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "eventSource",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.Integer,
                    Value = (int)logEvent.EventSource
                }
            });

            int objId = objectService.CreateORUpdateGeneralObject(svObject);
            return objId;
        }
    }
}