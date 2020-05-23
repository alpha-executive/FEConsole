using System;
using System.Collections.Generic;
using System.Text;

namespace FE.Creator.FEConsole.Shared.Models
{
   public class ReverseProxyConfig
    {
        public bool Enabled { get; set; }

        public string[] AllowedIPAddress { get; set; }
    }
}
