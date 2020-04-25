using System;
using System.Threading.Tasks;
using FE.Creator.FEConsole.Shared.Models;
using FE.Creator.ObjectRepository;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FE.Creator.FEConsoleAPI.Controllers
{
    public class ErrorController : FEAPIBaseController
    {
        IObjectService _objectService = null;
        public ErrorController(IObjectService objectService, 
            IServiceProvider provider):base(provider)
        {
            this._objectService = objectService;
        }

        [HttpGet]
        public async Task<FEGeneralError> Get()
        {
            FEGeneralError error = new FEGeneralError();
            var exceptionHandlerPathFeature =
                            HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            error.ErrorCode = StatusCodes.Status500InternalServerError;
            error.ErrorDescription = exceptionHandlerPathFeature?.Error.Message;

            if (!string.IsNullOrEmpty(exceptionHandlerPathFeature?.Error.Message))
            {
                AppEventModel errEvent = new AppEventModel();
                errEvent.EventTitle = "Application Error Happened";
                errEvent.EventDetails = exceptionHandlerPathFeature?.Error.Message;
                errEvent.EventOwner = await GetLoginUser();
                errEvent.EventLevel = AppEventModel.EnumEventLevel.Error;
                errEvent.EventDateTime = DateTime.Now;

                await LogAppEvent(_objectService, errEvent);
            }

            return error;
        }
    }
}