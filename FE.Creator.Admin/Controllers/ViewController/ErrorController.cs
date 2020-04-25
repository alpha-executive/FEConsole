using Microsoft.AspNetCore.Mvc;
using System;

namespace FE.Creator.Admin.Controllers.ViewController
{
    public class ErrorController : BaseController
    {
        public ErrorController(IServiceProvider serviceProvider) : base(serviceProvider) { }
        // GET: Error
        public ActionResult Page(int id)
        {
            return View("ErrorPage", id);
        }
    }
}