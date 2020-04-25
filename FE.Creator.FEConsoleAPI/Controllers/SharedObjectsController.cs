using FE.Creator.FEConsole.Shared.Services.FileStorage;
using FE.Creator.ObjectRepository;
using FE.Creator.ObjectRepository.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    public class SharedObjectsController : FEAPIBaseController
    {
        public IObjectService objectService = null;
        IFileStorageService storageService = null;
        public readonly ILogger<SharedObjectsController> logger = null;
        public SharedObjectsController(IObjectService objectService,
                IFileStorageService storageService,
                ILogger<SharedObjectsController> logger,
                IServiceProvider provider) : base(provider)
        {
            this.objectService = objectService;
            this.storageService = storageService;
            this.logger = logger;
        }

        [Route("[action]/{objectId}/{shareFieldName}")]
        [HttpPost]
        [ProducesResponseType(typeof(List<ServiceObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ServiceObject> GetSharedServiceObjectById(int objectId, string shareFieldName, [FromBody] string[] properties)
        {
            if (objectId < 0)
            {
                return NotFound();
            }
            var svcObj = objectService.GetServiceObjectById(objectId, properties, null);
            if (svcObj != null
                    && svcObj.GetPropertyValue<PrimeObjectField>(shareFieldName)
                                              .GetStrongTypeValue<int>() == 1)
            {
                return Ok(svcObj);
            }

            return NotFound();
        }

        [Route("[action]/{objDefName}/{shareFieldName}")]
        [HttpPost]
        [ProducesResponseType(typeof(List<ServiceObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<ServiceObject>> GetSharedServiceObjects(string objDefName,
            string shareFieldName,
            [FromBody] string[] properties,
            int page = 1, 
            int pageSize = 1)
        {
            var objDef = FindObjectDefinitionByName(objDefName);

            if (objDef == null || objDef.ObjectDefinitionID < 0)
            {
                return NotFound();
            }

            int objDefId = objDef.ObjectDefinitionID;
            List<ServiceObject> svcObjLists = GetSharedObjects(properties,    
                                                                page,
                                                                pageSize, 
                                                                objDefId);
            if(svcObjLists　!= null && svcObjLists.Count > 0)
            {
                var sharedObjects = (from s in svcObjLists
                                     where s.GetPropertyValue<PrimeObjectField>(shareFieldName)
                                              .GetStrongTypeValue<int>() == 1
                                     select s
                                  )
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();

                return Ok(sharedObjects);
            }

            return NotFound();
        }

        private List<ServiceObject> GetSharedObjects(string[] properties, int page, int pageSize, int objDefId)
        {
            var svcObjLists = objectService.GetServiceObjects(
                objDefId,
                properties,
                page,
                pageSize,
                null);

            return svcObjLists;
        }

        [Route("[action]/{objDefName}")]
        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public int GetSharedServiceObjectsCount(string objDefName,
          [FromBody] string shareFieldName)
        {
            var objDef = FindObjectDefinitionByName(objDefName);

            if (objDef == null || objDef.ObjectDefinitionID < 0)
            {
                return 0;
            }
            int objDefId = objDef.ObjectDefinitionID;
            List<ServiceObject> svcObjLists = GetSharedObjects(new string[] { shareFieldName },
                                                              1,
                                                              int.MaxValue,
                                                              objDefId);

            if (svcObjLists != null && svcObjLists.Count > 0)
            {
                var count = (from s in svcObjLists
                                     where s.GetPropertyValue<PrimeObjectField>(shareFieldName)
                                              .GetStrongTypeValue<int>() == 1
                                     select s
                                  )
                                  .Count();

                return count;
            }

            return 0;
        }

        private ObjectDefinition FindObjectDefinitionByName(string defname)
        {
            logger.LogDebug("Start FindObjectDefinitionByName");

            var objDefs = objectService.GetAllObjectDefinitions();

            logger.LogDebug("objDefs.count : " + objDefs.Count);

            var findObjDef = (from def in objDefs
                              where def.ObjectDefinitionName.Equals(defname, StringComparison.InvariantCultureIgnoreCase)
                              select def).FirstOrDefault();

            logger.LogDebug("End FindObjectDefinitionByName");
            return findObjDef;
        }


        [Route("[action]/{objectId}/{filePropertyName}/{shareFieldName}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DownloadSharedObjectFile(string objectId, 
            string filePropertyName, 
            string shareFieldName,
            bool thumbinal)
        {
            ServiceObject svo = objectService.GetServiceObjectById(int.Parse(objectId),
                new string[] { filePropertyName, shareFieldName },
                null);
            string file = string.Empty;
            if(svo != null && 
                svo.GetPropertyValue<PrimeObjectField>(shareFieldName)
                                            .GetStrongTypeValue<int>() == 1)
            {
                var downloadUrl = svo.GetPropertyValue<ObjectFileField>(filePropertyName)
                    .FileUrl;

                if (thumbinal)
                {
                    downloadUrl = string.Format("~{0}?thumbinal=true", downloadUrl);
                }

                return Redirect(downloadUrl);
            }

            return NotFound();
        }
    }
}
