using FE.Creator.ObjectRepository.EntityModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
   public class PrimeObjectField : ServiceObjectField
    {
        public PrimeFieldDataType PrimeDataType { get; set; }

        public object Value { get; set; }

        private object ConvertToTypeValue(Type type, string value)
        {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(type);
            object propValue = typeConverter.ConvertFromString(value);

            return propValue;
        }
        public T GetStrongTypeValue<T>(){
            return this.Value == null ? default(T) : 
                (T)ConvertToTypeValue(typeof(T), this.Value.ToString());
        }

        public override bool isFieldValueEqualAs(string v)
        {
            return (this.Value == null && string.IsNullOrEmpty(v)) 
                ||
                (this.Value != null && this.Value.ToString().Equals(v, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
