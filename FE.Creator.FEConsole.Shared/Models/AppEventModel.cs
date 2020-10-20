using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FE.Creator.FEConsole.Shared.Models
{
    public class AppEventModel
    {
        public enum EnumEventLevel
        {
            Error = 0,
            Warning = 1,
            Information = 2
        }


        public enum EventSourceEnum 
        { 
            Portal,
            Console
        }

        public AppEventModel() { EventSource = EventSourceEnum.Console; }
        public string EventTitle { get; set; }

        public string EventDetails { get; set; }

        public DateTime EventDateTime { get; set; }

        public EnumEventLevel EventLevel { get; set; }

        public string EventOwner { get; set; }

        public EventSourceEnum EventSource
        {
            get;set;
        }
    }
}