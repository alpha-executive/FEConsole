using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
   public class ObjectReferenceField : ServiceObjectField
    {
        public int ReferedGeneralObjectID { get; set; }

        public override bool isFieldValueEqualAs(string v)
        {
            return ReferedGeneralObjectID.ToString().Equals(v, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
