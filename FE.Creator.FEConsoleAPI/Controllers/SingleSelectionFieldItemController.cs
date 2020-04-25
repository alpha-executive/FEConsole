using FE.Creator.ObjectRepository;
using Microsoft.AspNetCore.Mvc;
using System;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    /// <summary>
    /// DELETE: api/SingleSelectionFieldItem/{id}
    ///      delete a selection item of the single selection field by id.
    /// </summary>
    public class SingleSelectionFieldItemController : FEAPIBaseController
    {
        IObjectService objectService = null;

        public SingleSelectionFieldItemController(IObjectService objectService,
            IServiceProvider provider) : base(provider)
        {
            this.objectService = objectService;
        }


        [Route("{id}")]
        [HttpDelete]
        // DELETE: api/SingleSelectionFieldItem/5
        public void Delete(int id)
        {
            this.objectService.DeleteSingleSelectionFieldSelectionItem(id);
        }
    }
}
