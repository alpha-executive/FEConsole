using FE.Creator.ObjectRepository;
using Microsoft.AspNetCore.Mvc;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    /// <summary>
    /// DELETE: api/SingleSelectionFieldItem/{id}
    ///      delete a selection item of the single selection field by id.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SingleSelectionFieldItemController : ControllerBase
    {
        IObjectService objectService = null;

        public SingleSelectionFieldItemController(IObjectService objectService)
        {
            this.objectService = objectService;
        }


        // DELETE: api/SingleSelectionFieldItem/5
        public void Delete(int id)
        {
            this.objectService.DeleteSingleSelectionFieldSelectionItem(id);
        }
    }
}
