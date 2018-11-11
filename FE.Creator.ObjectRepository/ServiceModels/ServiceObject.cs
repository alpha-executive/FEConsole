using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
   public class ServiceObject
    {
        public int ObjectDefinitionId { get; set; }

        public int ObjectID { get; set; }

        public string ObjectName { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime Updated { get; set; }

        public string UpdatedBy { get; set; }

        /// <summary>
        /// indicate that only save properties change to the database.
        /// </summary>
        public bool OnlyUpdateProperties { get; set; }


        /// <summary>
        /// for multi tanents cloud enviroment usage
        /// </summary>
        public string ObjectOwner { get; set; }

        private List<ObjectKeyValuePair> m_Properties = new List<ObjectKeyValuePair>();
        public List<ObjectKeyValuePair> Properties
        {
            get
            {
                return m_Properties;
            }
        }


        public ServiceObject Create(int objectDefintionId, string owner, string user)
        {
            ServiceObject o = new ServiceObject();
            o.Created = DateTime.UtcNow;
            o.Updated = DateTime.UtcNow;
            o.ObjectID = -1;
            o.ObjectDefinitionId = objectDefintionId;
            o.ObjectOwner = owner;
            o.UpdatedBy = user;
            o.CreatedBy = user;

            return o;
        }

        public void InsertProperty(string fieldName, ServiceObjectField field)
        {
            this.Properties.Add(
                    new ObjectKeyValuePair()
                    {
                      KeyName = fieldName,
                      Value = field
                    });
        }

        public T GetPropertyValue<T>(string property)
        {
            var kvp = (from k in this.Properties
                      where k.KeyName.Equals(property, StringComparison.InvariantCultureIgnoreCase)
                      select k).FirstOrDefault();

            return kvp != null ?  (T)kvp.Value : default(T);
        }
    }
}
