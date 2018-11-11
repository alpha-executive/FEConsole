using FE.Creator.ObjectRepository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
    [KnownType(typeof(SingleSDefinitionField))]
    [KnownType(typeof(PrimeDefinitionField))]
    [KnownType(typeof(ObjRefDefinitionField))]
  public class ObjectDefinitionField
    {
        public ObjectDefinitionField() { }

        public ObjectDefinitionField(GeneralObjectDefinitionFieldType fieldType)
        {
            this.GeneralObjectDefinitionFiledType = fieldType;
        }

        public int ObjectDefinitionFieldID { get; set; }

        public string ObjectDefinitionFieldName { get; set; }

        public string ObjectDefinitionFieldKey { get; set; }

        public GeneralObjectDefinitionFieldType GeneralObjectDefinitionFiledType { get; set; }
    }
}
