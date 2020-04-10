using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FE.Creator.FEConsole.Shared.Models
{
    public class LicensedModule
    {
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string ModuleDescription { get; set; }

        public DateTime ExpiredDate { get; set; }

        public bool Licensed { get; set; }
    }
}