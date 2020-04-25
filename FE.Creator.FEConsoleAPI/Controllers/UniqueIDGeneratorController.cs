using Microsoft.AspNetCore.Mvc;
using System;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    /// <summary>
    /// Get a unique identifier for client side use.
    /// GET: /api/UniqueIDGenerator
    ///    return: a new GUID string.
    /// </summary>
    public class UniqueIDGeneratorController : FEAPIBaseController
    {
        public UniqueIDGeneratorController(IServiceProvider provider) 
            : base(provider)
        {

        }
        // GET: api/UniqueIDGenerator
        [HttpGet]
        public string Get()
        {
            return Guid.NewGuid().ToString("D");
        }
    }
}
