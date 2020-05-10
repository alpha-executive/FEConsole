using System;
using System.Collections.Generic;
namespace FE.Creator.FEConsoleAPI.Controllers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using ObjectRepository;
    using ObjectRepository.ServiceModels;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// GET api/ObjectDefinitionGroup/GetByParentId/{id}
    ///     {id}: optional, the id of parent definition group.
    ///     return list of the definition groups, if {id} is not provided, return all the groups.
    /// GET: api/ObjectDefinitionGroup/{id}
    ///     {id}: required, the id of parent definition group.
    ///     return the specific object defintion group.
    /// POST: api/ObjectDefinitionGroup
    ///     create a new ObjectDefinitionGroup instance,required ObjectDefinitionGroup in the body.
    /// PUT: api/ObjectDefinitionGroup/{id}
    ///    {id}: required object definition group id.
    ///    update a new object definition group by group id.
    /// DELETE: api/ObjectDefinitionGroup/{id}
    ///    {id}: required object definition group id.
    ///    delete a object definition instance.
    /// </summary>
    public class ObjectDefinitionGroupController : FEAPIBaseController
    {
        IObjectService objectService = null;
        ILogger<ObjectDefinitionGroupController> logger = null;

        public ObjectDefinitionGroupController(IObjectService service,
            ILogger<ObjectDefinitionGroupController> logger,
            IServiceProvider provider) : base(provider)
        {
            this.objectService = service;
            this.logger = logger;
        }

        /// <summary>
        /// GET api/custom/ObjectDefinitionGroup/GetByParentId/{id}
        ///     {id}: optional, the id of parent definition group.
        ///     return list of the definition groups, if {id} is not provided, return all the groups.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("GetByParentId/{id?}")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ObjectDefinitionGroup>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ObjectDefinitionGroup>>> GetByParentId(int? id = null)
        {
            IEnumerable<ObjectDefinitionGroup> objDefGroups = await GetAllObjectDefinitionGroups(id);
#if !DEBUG
            objDefGroups = objDefGroups.Where(def =>
            {
                return !def.GroupName.Equals("FESystem")
                && !def.GroupKey.Equals("FE_SYS_APP_CONFIG");
            });
#endif

            return this.Ok(objDefGroups);
        }

        private Task<IEnumerable<ObjectDefinitionGroup>> GetAllObjectDefinitionGroups(int? parentGroupId)
        {
            return Task
                .FromResult<IEnumerable<ObjectDefinitionGroup>>(objectService.GetObjectDefinitionGroups(parentGroupId, 
                    null));
        }

        

        private Task<ObjectDefinitionGroup> GetObjectDefinitionGroup(int id)
        {
            return Task.FromResult<ObjectDefinitionGroup>(
                    objectService.GetObjectDefinitionGroupById(id)
                );
        }

        /// <summary>
        ///  GET: api/ObjectDefinitionGroup/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id:int}")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectDefinitionGroup), StatusCodes.Status200OK)]
        public async Task<ActionResult<ObjectDefinitionGroup>> Get(int id)
        {
            logger.LogDebug("Start Get : " + id);
            var objDefGroup = await GetObjectDefinitionGroup(id);

            logger.LogDebug(String.Format("get object definition group: {0} ", objDefGroup != null ? objDefGroup.GroupName : string.Empty));

            logger.LogDebug("End Get : " + id);
            return this.Ok(objDefGroup);
        }


        // POST: api/ObjectDefinitionGroup
        [HttpPost]
        [ProducesResponseType(typeof(ObjectDefinitionGroup), StatusCodes.Status201Created)]
        public ActionResult<ObjectDefinitionGroup> Post([FromBody]ObjectDefinitionGroup value)
        {
            logger.LogDebug("Start Post ObjectDefinitionGroup");
            if (value != null)
            {
               value.GroupID =  objectService.CreateOrUpdateObjectDefinitionGroup(value);
                logger.LogDebug("New Created ObjectDefinitionGroup : " + value.GroupID);
            }
            logger.LogDebug("End Post ObjectDefinitionGroup");
            return this.CreatedAtAction(nameof(Get), new { id = value.GroupID }, value);
        }

        // PUT: api/ObjectDefinitionGroup/5
        [Route("{id}")]
        [HttpPut]
        public void Put(int id, [FromBody]ObjectDefinitionGroup value)
        {
            logger.LogDebug("Start Put ObjectDefinitionGroup: " + id);
            if (value != null)
            {
                objectService.CreateOrUpdateObjectDefinitionGroup(value);
            }
            logger.LogDebug("End Put ObjectDefinitionGroup: " + id);
        }

        // DELETE: api/ObjectDefinitionGroup/5
        [Route("{id}")]
        [HttpDelete]
        public void Delete(int id)
        {
            logger.LogDebug("Start Delete ObjectDefinitionGroup: " + id);
            objectService.SoftDeleteObjectDefintionGroup(id);
            logger.LogDebug("End Delete ObjectDefinitionGroup: " + id);
        }
    }
}
