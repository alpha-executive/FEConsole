using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
   public class ObjectDefinitionGroup
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string GroupKey { get; set; }
        
        public ObjectDefinitionGroup ParentGroup { get; set; }

        private List<ObjectDefinitionGroup> m_ChildrenGroups = new List<ObjectDefinitionGroup>();
        public List<ObjectDefinitionGroup> ChidrenGroups
        {
            get { return m_ChildrenGroups; }
        }

    }
}
