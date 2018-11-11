using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
   public class ObjectDefinition
    {
        public int ObjectDefinitionID { get; set; }

        public String ObjectDefinitionName { get; set; }

        public String ObjectDefinitionKey { get; set; }

        public int ObjectDefinitionGroupID { get; set; }

        public bool IsFeildsUpdateOnly { get; set; }

        public string ObjectOwner { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        private List<ObjectDefinitionField> m_objectFields = new List<ObjectDefinitionField>();
        public List<ObjectDefinitionField> ObjectFields
        {
            get
            {
                return m_objectFields;
            }
        }
    }
}
