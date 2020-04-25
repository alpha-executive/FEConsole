
using Microsoft.AspNetCore.Mvc;
using System;

namespace FE.Creator.Admin.Controllers
{
    public class BaseController : Controller
    {
        private readonly IServiceProvider _serviceProvider = null;

        public BaseController(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }
    }
}