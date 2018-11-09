using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.EntityModels
{
    public enum PrimeFieldDataType
    {
        String,
        Integer,
        Long,
        Datetime,
        Number,
        Binary
    }

    internal class PrimeObjectDefintionField:GeneralObjectDefinitionField
    {
        public PrimeFieldDataType PrimeDataType { get; set; }
    }
}
