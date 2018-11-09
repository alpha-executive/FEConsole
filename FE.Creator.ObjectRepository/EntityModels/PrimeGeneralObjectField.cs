using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.EntityModels
{
    internal partial class PrimeGeneralObjectField : GeneralObjectField
    {
        public PrimeGeneralObjectField()
        {
            this.DateTimeValue = null;
        }
        public string StringValue { get; set; }

        public int? IntegerValue { get; set; }

        public long? LongValue { get; set; }

        public DateTime? DateTimeValue { get; set; }

        public double? NumberValue { get; set; }

        public byte[] BinaryValue { get; set; }
    }
}
