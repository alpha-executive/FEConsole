using FE.Creator.ObjectRepository.EntityModels;
using FE.Creator.ObjectRepository.ServiceModels;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.FEConsoleAPI.MVCExtension
{
    public class ObjectDefintionConverter : JsonCreationConverter<ObjectDefinition>
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        protected override ObjectDefinition Create(Type objectType, JObject jObject)
        {
            if (objectType != typeof(ObjectDefinition))
                throw new InvalidCastException("Not supported type: " + objectType.ToString());

            ObjectDefinition def = new ObjectDefinition();
            //def.ObjectDefinitionName = jObject.Value<string>("objectDefinitionName");
            //def.ObjectDefinitionKey = jObject.Value<string>("objectDefinitionKey");
            //def.ObjectDefinitionID = jObject.Value<int>("objectDefinitionID");
            //def.UpdatedBy = jObject.Value<string>("updatedBy");
            //def.ObjectOwner = jObject.Value<string>("objectOwner");
            //def.ObjectDefinitionGroupID = jObject.Value<int>("objectDefinitionGroupID");
            //def.IsFeildsUpdateOnly = jObject.Value<bool>("isFeildsUpdateOnly");
            //def.Created = jObject.Value<DateTime>("created");
            //def.Updated = jObject.Value<DateTime>("updated");

            return def;

        }


        protected override void AfterPopulate(ObjectDefinition target, JObject jObject)
        {
            target.ObjectFields.Clear();
            DeserializeFields(target.ObjectFields, jObject["objectFields"]); 
        }

        private void DeserializeFields(List<ObjectDefinitionField> fields, IEnumerable<JToken> jObjects)
        {
            foreach (JObject obj in jObjects)
            {
                ObjectDefinitionField field = null;
                var fieldType = obj.Value<int>("generalObjectDefinitionFiledType");
                switch ((GeneralObjectDefinitionFieldType)fieldType)
                {
                    case GeneralObjectDefinitionFieldType.PrimeType:
                        field = new PrimeDefinitionField();
                        ((PrimeDefinitionField)field).PrimeDataType = (PrimeFieldDataType)obj.Value<int>("primeDataType");
                        break;
                    case GeneralObjectDefinitionFieldType.ObjectReference:
                        field = new ObjRefDefinitionField();
                        ((ObjRefDefinitionField)field).ReferedObjectDefinitionID = obj.Value<int>("referedObjectDefinitionID");
                        break;
                    case GeneralObjectDefinitionFieldType.SingleSelection:
                        field = new SingleSDefinitionField();
                        ((SingleSDefinitionField)field).SelectionItems.AddRange(ParseSelectionItems(obj));
                        break;
                    default:
                        field = new ObjectDefinitionField((GeneralObjectDefinitionFieldType)fieldType);
                        break;
                }

                if (field != null)
                {
                    field.ObjectDefinitionFieldID = obj.Value<int>("objectDefinitionFieldID");
                    field.ObjectDefinitionFieldKey = obj.Value<string>("objectDefinitionFieldKey");
                    field.ObjectDefinitionFieldName = obj.Value<string>("objectDefinitionFieldName");
                    fields.Add(field);
                }
            }
        }

        private static List<DefinitionSelectItem> ParseSelectionItems(JObject obj)
        {
            List<DefinitionSelectItem> selList = new List<DefinitionSelectItem>();
            if (obj != null && obj["selectionItems"] != null)
            {
                foreach(JObject item in obj["selectionItems"])
                {
                    DefinitionSelectItem it = new DefinitionSelectItem();
                    it.SelectItemID = item.Value<int>("selectItemID");
                    it.SelectDisplayName = item.Value<string>("selectDisplayName");
                    it.SelectItemKey = item.Value<string>("selectItemKey");

                    selList.Add(it);
                }
            }

            return selList;
        }
    }
    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        /// <summary>
        /// Create an instance of objectType, based properties in the JSON object
        /// </summary>
        /// <param name="objectType">type of object expected</param>
        /// <param name="jObject">
        /// contents of JSON object that will be deserialized
        /// </param>
        /// <returns></returns>
        protected abstract T Create(Type objectType, JObject jObject);


        protected abstract void AfterPopulate(T target, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader,
                                        Type objectType,
                                         object existingValue,
                                         JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            T target = Create(objectType, jObject);

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            //perform the after populate operation.
            AfterPopulate(target, jObject);

            return target;
        }
    }

    public class ObjectDefintionFormatter: TextInputFormatter
    {
        public ObjectDefintionFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanReadType(Type type)
        {
            if (type == typeof(ObjectDefinition))
            {
                return base.CanReadType(type);
            }
            return false;
        }

        public object ReadFromStream(Type type, Stream readStream, Encoding effectiveEncoding)
        {
            using (StreamReader reader = new StreamReader(readStream, effectiveEncoding))
            {
                if (type == typeof(ObjectDefinition))
                {
                    //deal with object defintion
                    return JsonConvert.DeserializeObject(reader.ReadToEnd(), type, new ObjectDefintionConverter());
                }

                return JsonConvert.DeserializeObject(reader.ReadToEnd(), type);
            }
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            var request = context.HttpContext.Request;

            using (var reader = new StreamReader(request.Body, encoding))
            {
                try
                {
                    string content = await reader.ReadToEndAsync();
                    var convertObject = JsonConvert.DeserializeObject<ObjectDefinition>(content,
                        new ObjectDefintionConverter());

                    return await InputFormatterResult.SuccessAsync(convertObject);
                }
                catch
                {
                    return await InputFormatterResult.FailureAsync();
                }
            }
        }
    }
}