using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
   public class ObjectKeyValuePair
    {
        public string KeyName { get; set; }

        public object Value { get; set; }

        public bool IsArray { get; set; }
    }
}
