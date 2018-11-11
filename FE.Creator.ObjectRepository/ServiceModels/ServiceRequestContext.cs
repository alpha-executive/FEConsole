using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
  public class ServiceRequestContext
    {
        public string RequestUser { get; set; }
        public bool IsDataCurrentUserOnly { get; set; }

        public bool UserSenstiveForSharedData { get; set; }
    }
}
