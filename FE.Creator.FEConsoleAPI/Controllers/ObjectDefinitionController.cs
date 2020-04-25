using System;
using System.Collections.Generic;
using System.Linq;

namespace FE.Creator.FEConsoleAPI.Controllers
{
    using System.Threading.Tasks;
    using FE.Creator.ObjectRepository;
    using FE.Creator.ObjectRepository.ServiceModels;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MVCExtension;
    /// <summary>
    ///  GET api/objectdefinitions/list/{groupname}
    ///      {groupname}: optional, group name, if not specified, return all the object defintions.
    ///      return all the object defintions of current group specified by {groupname}
    ///  GET  api/ObjectDefinition/GetAllDefinitions
    ///      return all the object definitions in the system.
    ///  GET api/ObjectDefinition/FindObjectDefintionsByGroup/{id}
    ///      {id}: optional, group id, if not specified, return all the object definitions
    ///      return:  return all the object defintions of current group specified by {id}: group id.
    ///   GET: api/ObjectDefinition/getSystemObjectDefinitions
    ///       get system build-in object definitions.
    ///   GET: api/ObjectDefinition/getCustomObjectDefinitions
    ///       get all the custom defined object definitions. 
    ///  GET: api/ObjectDefinition/{id}
    ///      {id}: required, object definition id.
    ///      return: return the specific definition by id.
    ///  POST api/ObjectDefinition
    ///     create a object definition instance, required ObjectDefinition json parameter in the body.
    ///  PUT: api/ObjectDefinition/{id}
    ///     {id}, required object definition id.
    ///     update a object definition instance. required ObjectDefinition json parameter in the body.
    ///  DELETE: api/ObjectDefinition/{id}
    ///     delete a object definition by {id}
    /// </summary>

    public class ObjectDefinitionController : FEAPIBaseController
    {
        IObjectService objectService = null;
        ILogger<ObjectDefinitionController> logger = null;
        readonly IConfiguration _configuration;

        public ObjectDefinitionController(IObjectService objectService,
             IConfiguration configuration,
             ILogger<ObjectDefinitionController> logger, 
             IServiceProvider provider) : base(provider)
        {
            this.objectService = objectService;
            this.logger = logger;
            this._configuration = configuration;
        }

        private Task<IEnumerable<ObjectDefinition>> getObjectDefinitions()
        {
            logger.LogDebug("Start getObjectDefinitions");
            var objDefinitions = objectService.GetAllObjectDefinitions();
            logger.LogDebug(string.Format("count of the object definitions: {0}", objDefinitions != null ? objDefinitions.Count : 0));

            logger.LogDebug("End getObjectDefinitions");
            return Task.FromResult<IEnumerable<ObjectDefinition>>(objDefinitions);
        }


        /// <summary>
        /// api/objectdefinition/list/{groupname}
        /// </summary>
        /// <param name="groupname"></param>
        /// <returns></returns>
        [Route("list/{groupname?}")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ObjectDefinition>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ObjectDefinition>>> List(string groupname = null)
        {
            logger.LogDebug("Start ObjectDefinitionController.List");
            int groupId = -1;
            if (!string.IsNullOrEmpty(groupname))
            {
                var groups = objectService.GetObjectDefinitionGroups(null, null);
                var foundGroup = (from g in groups
                                  where g.GroupName.Equals(groupname, StringComparison.InvariantCultureIgnoreCase)
                                  select g).FirstOrDefault();

                groupId = foundGroup != null ? foundGroup.GroupID : -1;
                logger.LogDebug("groupId = " + groupId);
            }

            logger.LogDebug("End ObjectDefinitionController.List");

            if (groupId == -1)
                return await this.FindObjectDefintionsByGroup();
            else
                return await this.FindObjectDefintionsByGroup(groupId);
        }

        // GET: api/ObjectDefinition/GetAllDefinitions
        [Route("GetAllDefinitions")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ObjectDefinition>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ObjectDefinition>>> GetAllDefinitions()
        {
            var objDefintions = await getObjectDefinitions();
            return this.Ok(objDefintions);
        }

        // GET: api/ObjectDefinition/getSystemObjectDefinitions
        [Route("getSystemObjectDefinitions")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ObjectDefinition>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ObjectDefinition>> getSystemObjectDefinitions()
        {
            logger.LogDebug("Start ObjectDefinitionController.getSystemObjectDefinitions");
            var groups = objectService.GetObjectDefinitionGroups(null, null);
            var sysGroup = (from g in groups
                            where g.GroupName.Equals("FESystem", StringComparison.InvariantCultureIgnoreCase)
                            select g).FirstOrDefault();
            logger.LogDebug("FESystem group found ? {0}", sysGroup != null);

            if (sysGroup != null)
            {
#if !LogDebug
                var objDefinitions = objectService.GetObjectDefinitionsByGroup(sysGroup.GroupID, 
                    1, 
                    int.MaxValue,
                    null);
#else
                var objDefinitions = objectService.GetAllObjectDefinitions();
#endif
                logger.LogDebug("objDefinitions.Count = " + objDefinitions.Count);
                return this.Ok(objDefinitions);
            }

            logger.LogError("FESystem group not found, System is not in correct status, check license or database status.");
            return this.NotFound();
        }

        // GET: api/ObjectDefinition/getCustomObjectDefinitions
        [Route("getCustomObjectDefinitions")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ObjectDefinition>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ObjectDefinition>>> getCustomObjectDefinitions()
        {
            logger.LogDebug("Start ObjectDefinitionController.getCustomObjectDefinitions");
            var groups = objectService.GetObjectDefinitionGroups(null, null);
            var sysGroup = (from g in groups
                              where g.GroupName.Equals("FESystem", StringComparison.InvariantCultureIgnoreCase)
                              select g).FirstOrDefault();
            logger.LogDebug("FESystem group found ? {0}", sysGroup != null);

            if(sysGroup!= null)
            {
                var objDefinitions = objectService.GetObjectDefinitionsExceptGroup(sysGroup.GroupID,
                    new ServiceRequestContext()
                    {
                        IsDataCurrentUserOnly = bool.Parse(_configuration["SiteSettings:IsDataForLoginUserOnly"]),
                        RequestUser = await GetLoginUser(),
                        UserSenstiveForSharedData = false
                    });
                logger.LogDebug("objDefinitions.Count = " + objDefinitions.Count);
                return this.Ok(objDefinitions);
            }

            logger.LogError("FESystem group not found, System is not in correct status, check license or database status.");

            return this.NotFound();
        }
        /// <summary>
        ///  GET api/custom/ObjectDefinition/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("FindObjectDefintionsByGroup/{id?}")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ObjectDefinition>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ObjectDefinition>>> FindObjectDefintionsByGroup(int? id = null)
        {
            var objDefinitions = id.HasValue ?
                objectService.GetObjectDefinitionsByGroup(id.Value,
                1, 
                int.MaxValue,
                new ServiceRequestContext()
                {
                    IsDataCurrentUserOnly = bool.Parse(_configuration["SiteSettings:IsDataForLoginUserOnly"]),
                    RequestUser = await GetLoginUser()
                }) :
                await getObjectDefinitions();

            if(objDefinitions == null)
            {
                return NotFound();
            }

            return this.Ok(objDefinitions);
        }

        private Task<ObjectDefinition> getObjectDefinition(int id)
        {
            var objectDefinition = objectService.GetObjectDefinitionById(id);

            return Task.FromResult<ObjectDefinition>(objectDefinition);
        }

        [Route("FindObjectDefinition/{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectDefinition), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // GET: api/ObjectDefinition/5
        public async Task<ActionResult<ObjectDefinition>> FindObjectDefinition(int id)
        {
            var objDefinition = await getObjectDefinition(id);

            return this.Ok(objDefinition);
        }

        bool IsValidObjectDefinitoinExists(string defname)
        {
            var objDefs = objectService.GetAllObjectDefinitions();
            logger.LogDebug("objDefs.count : " + objDefs.Count);

            var findObjDef = (from def in objDefs
                              where def.ObjectDefinitionName.Equals(defname, StringComparison.InvariantCultureIgnoreCase)
                              select def).FirstOrDefault();

            return findObjDef != null;
        }

        // POST: api/ObjectDefinition
        [HttpPost]
        [ProducesResponseType(typeof(ObjectDefinition), StatusCodes.Status200OK)]
        public async Task<ActionResult<ObjectDefinition>> Post([FromBody]ObjectDefinition value)
        {
            logger.LogDebug("Start Post");
            int objectId = -1;
            if(value != null)
            {
                value.ObjectOwner = await GetLoginUser();
                value.UpdatedBy = value.ObjectOwner;

                if(!string.IsNullOrEmpty(value.ObjectDefinitionName)
                    && !IsValidObjectDefinitoinExists(value.ObjectDefinitionName))
                {
                    objectId = objectService.CreateORUpdateObjectDefinition(value);
                    logger.LogDebug("New object defintion with objectId = " + objectId);
                }
                else
                {
                    throw new InvalidOperationException("object definition name already exists.");
                }
            }

            logger.LogDebug("End Post");
            return await this.FindObjectDefinition(objectId);
        }

        // PUT: api/ObjectDefinition/5
        [Route("{id}")]
        [HttpPut]
        public async Task Put(int id, [FromBody]ObjectDefinition value)
        {
            if(value != null)
            {
                value.UpdatedBy = await GetLoginUser();
                if (string.IsNullOrEmpty(value.ObjectOwner))
                {
                    value.ObjectOwner = value.UpdatedBy;
                }

                objectService.CreateORUpdateObjectDefinition(value);
            }
        }

        // DELETE: api/ObjectDefinition/5
        [Route("{id}")]
        [HttpDelete]
        public void Delete(int id)
        {
            logger.LogDebug("Start Delete");
            objectService.DeleteObjectDefinition(id);
            logger.LogDebug("object definition id = " + id + " was deleted");
            logger.LogDebug("End Delete");
        }
    }
}
