using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemUserController : ControllerBase
    {
        private readonly ILogger<SystemUserController> logger = null;
        readonly IConfiguration _configuration;
        public SystemUserController(ILogger<SystemUserController> logger,
            IConfiguration configuration)
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
                    /*from u in this.UserManager.Users
                        select new
                        {
                            Id = u.Id,
                            UserName = u.UserName,
                            Email = u.Email,
                            EmailConfirmed = u.EmailConfirmed,
                            AccessFailedCount = u.AccessFailedCount,
                        };*/
            logger.LogDebug("User count: " + users.Count());
            logger.LogDebug("End List");
            return this.Ok(
                    users
                );
        }

        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public ActionResult<string> GetUserIdByUserLoginName()
        {
            var userId = "sample";
                    /*(from u in this.UserManager.Users
                        where u.UserName.Equals(this.User.Identity.Name)
                        select u.Id).FirstOrDefault();*/
            logger.LogDebug("Login User ID: " + userId);
            return this.Ok(userId);
        }

        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public string GetAdminLoginName()
        {
            var adminUser = _configuration["SiteSettings:SuperAdmin"];

            return adminUser;
        }

        private String GetResetPassword()
        {
            return "13345353";
        }

        [HttpPost]
        public void ResetUserPassword(string id)
        {
            logger.LogDebug("Start ResetUserPassword");

            if(User.Identity.Name.Equals(
                _configuration["SiteSettings:SuperAdmin"], StringComparison.CurrentCultureIgnoreCase))
            {
                throw new UnauthorizedAccessException("Permission Required to Reset password : " + User.Identity.Name);
            }

            if (!string.IsNullOrEmpty(id))
            {
                //var user = await this.UserManager.FindByIdAsync(id);
                //if (user != null)
                //{
                //    var token = await this.UserManager.GeneratePasswordResetTokenAsync(user.Id);
                //    await this.UserManager.ResetPasswordAsync(user.Id, token, GetResetPassword());
                //}
            }
            logger.LogDebug("End ResetUserPassword");
        }
    }
}
