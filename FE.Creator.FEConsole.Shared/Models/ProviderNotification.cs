using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FE.Creator.FEConsole.Shared.Models
{
    public class ProviderNotification
    {
        public string NotifyDesc { get; set; }
        public string ImageSrc { get; set; }

        public string ActionUrl { get; set; }

        public string Notifier { get; set; }

        public string EventTime { get; set; }
    }
}