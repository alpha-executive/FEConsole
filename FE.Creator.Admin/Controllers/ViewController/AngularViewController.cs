using FE.Creator.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.RegularExpressions;

namespace FE.Creator.Admin.ViewController.Controllers
{
    [Route("ngview")]
    public class AngularViewController : BaseController
    {
        public AngularViewController(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [Route("{module}/{name}")]
        [HttpGet]
        public ActionResult ClientTemplate(string module, string name)
        {
            if (name == null || !Regex.IsMatch(name, @"^[-\w]+$"))
                throw new ArgumentException("Illegal template name", "name");

            if (module == null || !Regex.IsMatch(module, @"^[-\w]+$"))
                throw new ArgumentException("Illegal template name", "name");
            
            return View(string.Format("~/Views/AngularView/Client/{0}/{1}.cshtml", module, name));
        }

        [Route("[action]/{module}/{name}")]
        [HttpGet]
        public ActionResult Index(string module, string name)
        {
            if (name == null || !Regex.IsMatch(name, @"^[-\w]+$"))
                throw new ArgumentException("Illegal template name", "name");

            if (module == null || !Regex.IsMatch(module, @"^[-\w]+$"))
                throw new ArgumentException("Illegal template name", "name");

            ViewBag.Title = module;
            return View(string.Format("~/Views/AngularView/Server/{0}/{1}.cshtml", module, name));
        }

        [Route("[action]/{module}/{name}/{Id}")]
        [HttpGet]
        public ActionResult EditOrDisplay(string module, string name, string Id)
        {
            if (name == null || !Regex.IsMatch(name, @"^[-\w]+$"))
                throw new ArgumentException("Illegal template name", "name");

            if (module == null || !Regex.IsMatch(module, @"^[-\w]+$"))
                throw new ArgumentException("Illegal template name", "name");

            ViewBag.Title = module;
            return View(string.Format("~/Views/AngularView/Server/{0}/{1}.cshtml", module, name), Id);
        }
    }
}