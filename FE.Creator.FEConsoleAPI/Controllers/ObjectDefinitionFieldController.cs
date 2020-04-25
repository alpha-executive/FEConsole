using FE.Creator.ObjectRepository;
using Microsoft.AspNetCore.Mvc;
using System;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    /// <summary>
    /// DELETE: api/ObjectDefinitionField/{id}
    ///      delete a object definition field by id
    /// </summary>
    public class ObjectDefinitionFieldController : FEAPIBaseController
    {
        IObjectService objectService = null;


        public ObjectDefinitionFieldController(IObjectService objectService,
            IServiceProvider provider) : base(provider)
        {
            this.objectService = objectService;
        }

        // DELETE: api/ObjectDefinitionField/5
        [Route("{id:int}")]
        [HttpDelete]
        public void Delete(int id)
        {
            objectService.DeleteObjectDefinitionField(id);
        }
    }
}
