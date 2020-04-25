using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using FE.Creator.ObjectRepository.ServiceModels;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    public class SystemUserController : FEAPIBaseController
    {
        private readonly ILogger<SystemUserController> logger = null;
        readonly IConfiguration _configuration;
        public SystemUserController(ILogger<SystemUserController> logger,
            IConfiguration configuration,
            IServiceProvider provider) : base(provider)
        {
            this.logger = logger;
            this._configuration = configuration;
        }

        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<object>> List(int? pageIndex = 1, int? pageSize = int.MaxValue)
        {
            logger.LogDebug("Start List");
            var users = new List<string>();
            logger.LogDebug("User count: " + users.Count());
            logger.LogDebug("End List");
            return this.Ok(
                    users
                );
        }

        [Route("[action]/{profile}/{attrs}")]
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<ActionResult<ServiceObject>> GetUserProfile(string profile, string attrs)
        {   
            var loginUser = await GetLoginUserId();
            logger.LogDebug("Login User ID: " + loginUser);

            return RedirectToAction("FindServiceObjectsByFilter", "GeneralObject", new {
                @definitionname = profile,
                @parameters = attrs,
                filters = "userExternalId," + loginUser
            });
        }

        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public string GetAdminLoginName()
        {
            var adminUser = _configuration["SiteSettings:SuperAdmin"];

            return adminUser;
        }
    }
}
