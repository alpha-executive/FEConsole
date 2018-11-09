using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.EntityModels
{
    public enum GeneralObjectDefinitionFieldType
    {
        //string, int, number (double), date
        PrimeType,
        ObjectReference,
        SingleSelection,
        File
    }

    internal class GeneralObjectDefinitionField
    {
        public int GeneralObjectDefinitionFieldID { get; set; }

        public string GeneralObjectDefinitionFieldName { get; set; }

        public string GeneralObjectDefinitionFieldKey { get; set; }

        public GeneralObjectDefinitionFieldType GeneralObjectDefinitionFiledType { get; set; }

        public int GeneralObjectDefinitionID { get; set; }

        public virtual GeneralObjectDefinition GeneralObjectDefinition { get; set; }
    }
}
