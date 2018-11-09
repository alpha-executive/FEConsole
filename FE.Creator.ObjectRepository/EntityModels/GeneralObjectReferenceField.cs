using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.EntityModels
{
    internal class GeneralObjectReferenceField : GeneralObjectField
    {
        public int ReferedGeneralObjectID { get; set; }
    }
}
