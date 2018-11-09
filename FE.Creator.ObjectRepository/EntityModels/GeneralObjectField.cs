using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.EntityModels
{
    internal class GeneralObjectField
    {
        public int GeneralObjectFieldID { get; set; }

        public int GeneralObjectDefinitionFieldID { get; set; }

        public virtual GeneralObjectDefinitionField GeneralObjectDefinitionField { get; set; }

        public int GeneralObjectID { get; set; }

        public virtual GeneralObject GeneralObject { get; set; }
    }
}
