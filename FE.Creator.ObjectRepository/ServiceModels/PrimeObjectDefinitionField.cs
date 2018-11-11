using FE.Creator.ObjectRepository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
    public class PrimeDefinitionField : ObjectDefinitionField
    {
        public PrimeDefinitionField() : base(GeneralObjectDefinitionFieldType.PrimeType) { }

        public PrimeFieldDataType PrimeDataType { get; set; }
    }
}
