﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FE.Creator.FEConsole.Shared;
using FE.Creator.FEConsole.Shared.Models;
using FE.Creator.ObjectRepository;
using FE.Creator.ObjectRepository.EntityModels;
using FE.Creator.ObjectRepository.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FE.Creator.FEConsoleAPI.Controllers
{
    public class SiteAdminController : FEAPIBaseController
    {
        private readonly IObjectService objectService = null;
        private readonly ILogger<SiteAdminController> logger = null;
        readonly IConfiguration _configuration;

        public SiteAdminController(IObjectService objectService,
            IConfiguration configuration,
            ILogger<SiteAdminController> logger,
            IServiceProvider provider):base(provider)
        {
            this.objectService = objectService;
            this._configuration = configuration;
            this.logger = logger;
        }
        protected int GetAppObjectDefintionIdByName(string defName)
        {
            var objDefs = objectService.GetAllObjectDefinitions();
            if (objDefs == null
                || objDefs.Count == 0)
                return -1;

            var findObjDef = (from def in objDefs
                              where def.ObjectDefinitionName.Equals(defName, StringComparison.InvariantCultureIgnoreCase)
                              select def).FirstOrDefault();

            return findObjDef.ObjectDefinitionID;
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<int> LogEvent(AppEventModel logEvent)
        {
            int objId = await LogAppEvent(objectService, logEvent);

            return objId;
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
        public ActionResult<Dictionary<string, string>> GetAppConfig(List<string> configureKeys)
        {
            Dictionary<string, string> configs = new Dictionary<string, string>();
            logger.LogDebug("Start getProviderNotificationData");

            var email = User.FindFirstValue("email");

            if (configureKeys != null 
                && configureKeys.Count > 0)
            {
                logger.LogDebug("# of the queried configure keys: " + configureKeys.Count);

                int settingsDefId = GetAppObjectDefintionIdByName("AppConfig");
                logger.LogDebug("settingsDefId = " + settingsDefId);
                var settings = objectService.GetServiceObjects(settingsDefId,
                    configureKeys.ToArray(),
                    1,
                    1,
                    null);
                if(settings != null && settings.Count > 0)
                {
                    foreach (var key in configureKeys)
                    {
                        string value = settings
                            .First()
                            .GetPropertyValue<PrimeObjectField>(key)
                            .GetStrongTypeValue<string>();

                        configs.Add(key, value);
                    }
                }
            }
            

            return configs;
        }

        [Route("[action]/{count}")]
        [HttpGet]
        [ProducesResponseType(typeof(List<AppEventModel>), StatusCodes.Status200OK)]
        public async Task<List<AppEventModel>> LatestSystemEvent(int count)
        {
            int eventDefId = GetAppObjectDefintionIdByName("AppEvent");
            logger.LogDebug("eventDefId = " + eventDefId);

            var events = objectService.GetServiceObjects(eventDefId,
               new string[] { "eventTitle", "eventDetails", "eventDateTime", "eventLevel", "eventOwner" },
               1,
               count > 0 ? count : 10,
               new ServiceRequestContext()
               {
                  IsDataCurrentUserOnly = bool.Parse(_configuration["SiteSettings:IsDataForLoginUserOnly"]),
                  RequestUser = await GetLoginUser(),
                  UserSenstiveForSharedData = false
               });

            List<AppEventModel> latestEvents = new List<AppEventModel>();
            if (events != null && events.Count > 0)
            {
                foreach (var evt in events)
                {
                    latestEvents.Add(new AppEventModel()
                    {
                        EventTitle = evt.GetPropertyValue<PrimeObjectField>("eventTitle").GetStrongTypeValue<string>(),
                        EventDetails = evt.GetPropertyValue<PrimeObjectField>("eventDetails").GetStrongTypeValue<string>(),
                        EventDateTime = evt.GetPropertyValue<PrimeObjectField>("eventDateTime").GetStrongTypeValue<DateTime>(),
                        EventLevel = (AppEventModel.EnumEventLevel)evt.GetPropertyValue<PrimeObjectField>("eventLevel").GetStrongTypeValue<int>(),
                        EventOwner = evt.GetPropertyValue<PrimeObjectField>("eventOwner").GetStrongTypeValue<string>()
                    });
                }
            }

            return latestEvents;
        }

        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(List<ProviderNotification>), StatusCodes.Status200OK)]
        public List<ProviderNotification> RecentSystemNotifications()
        {
            List<ProviderNotification> notifies = new List<ProviderNotification>();
            logger.LogDebug("get any notification from provider site, will use the default one");
            
            //if have notifications, please add it here.
            //var notifyData = new ProviderNotification
            //{
            //    NotifyDesc = "",
            //    ImageSrc = "~/Content/adminlte-2.3.6/dist/img/alarm-50.png",
            //    ActionUrl = "#",
            //    Notifier = "FETechHub",
            //    EventTime = DateTime.Now.ToShortDateString()
            //};

            //notifies.Add(notifyData);
            return notifies;
        }

    }
}