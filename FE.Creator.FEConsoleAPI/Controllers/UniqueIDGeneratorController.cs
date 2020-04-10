using Microsoft.AspNetCore.Mvc;
using System;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    /// <summary>
    /// Get a unique identifier for client side use.
    /// GET: /api/UniqueIDGenerator
    ///    return: a new GUID string.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UniqueIDGeneratorController : ControllerBase
    {
        // GET: api/UniqueIDGenerator
        [HttpGet]
        public string Get()
        {
            return Guid.NewGuid().ToString("D");
        }
    }
}
