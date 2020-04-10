using FE.Creator.ObjectRepository;
using Microsoft.AspNetCore.Mvc;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    /// <summary>
    /// DELETE: api/ObjectDefinitionField/{id}
    ///      delete a object definition field by id
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectDefinitionFieldController : ControllerBase
    {
        IObjectService objectService = null;


        public ObjectDefinitionFieldController(IObjectService objectService)
        {
            this.objectService = objectService;
        }

        // DELETE: api/ObjectDefinitionField/5
        [HttpDelete]
        public void Delete(int id)
        {
            objectService.DeleteObjectDefinitionField(id);
        }
    }
}
