using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
   public class ObjRefDefinitionField : ObjectDefinitionField
    {
        public ObjRefDefinitionField() : base(EntityModels.GeneralObjectDefinitionFieldType.ObjectReference) { }

        public int ReferedObjectDefinitionID { get; set; }
    }
}
