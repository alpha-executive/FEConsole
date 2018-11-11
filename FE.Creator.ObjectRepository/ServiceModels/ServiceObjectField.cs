using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
   public abstract class ServiceObjectField
    {
        public ServiceObjectField()
        {
            this.ObjectFieldID = -1;
        }

        public int ObjectFieldID { get; set; }

        public abstract bool isFieldValueEqualAs(string v);
    }
}
