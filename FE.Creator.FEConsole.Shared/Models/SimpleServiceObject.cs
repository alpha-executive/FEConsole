using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace FE.Creator.FEConsole.Shared.Models
{
    public class FileFiledValue
    {
        public string fileName { get; set; }

        public string fileUrl { get; set; }

        public string fileExtension { get; set; }

        public int fileSize { get; set; }
    }
    public class FieldValue
    {
        public object value { get; set; }

        [JsonExtensionData]
        private IDictionary<string,JToken> _additionalData;

        public FileFiledValue FileValue {get;set;}

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_additionalData.ContainsKey("fileUrl"))
            {
                if (this.FileValue == null)
                {
                    this.FileValue = new FileFiledValue();
                }

                this.FileValue.fileName = (string)_additionalData["fileName"];
                this.FileValue.fileSize = (int)_additionalData["fileSize"];
                this.FileValue.fileUrl = (string)_additionalData["fileUrl"];
                this.FileValue.fileExtension = (string)_additionalData["fileExtension"];
            }
        }
    }
    public class ServiceObjectField
    {
        public string keyName { get; set; }
        public FieldValue value { get; set; }
    }
    public class SimpleServiceObject
    {
        public int objectID { get; set; }

        public string objectName { get; set; }

        public DateTime updated { get; set; }

        public string updatedBy { get; set; }

        public string objectOwner { get; set; }
        public IList<ServiceObjectField> properties { get; set; }

        public T GetPropertyValue<T>(string propName)
        {
            foreach(var p in properties)
            {
                if(p.keyName.Equals(propName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if(p.value.value != null)
                    {
                        return (T)(p.value.value);
                    }
                }
            }

            return default(T);
        }

        public FileFiledValue GetPropertyFileValue(string propName)
        {
            foreach (var p in properties)
            {
                if (p.keyName.Equals(propName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (p.value.FileValue != null)
                    {
                        return p.value.FileValue;
                    }
                }
            }

            return null;
        }


    }
}
