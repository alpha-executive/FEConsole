using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.EntityModels
{
    internal class GeneralObjectDefinitionGroup
    {
        public GeneralObjectDefinitionGroup()
        {
            this.ChildrenGroups = new HashSet<GeneralObjectDefinitionGroup>();
            this.GeneralObjectDefinitions = new HashSet<GeneralObjectDefinition>();
        }
        public int GeneralObjectDefinitionGroupID { get; set; }
        public string GeneralObjectDefinitionGroupName { get; set; }
        public string GeneralObjectDefinitionGroupKey { get; set; }

        public bool IsDeleted { get; set; }

        public virtual GeneralObjectDefinitionGroup ParentGroup { get; set; }
        public virtual ICollection<GeneralObjectDefinitionGroup> ChildrenGroups { get; set; }

        public virtual ICollection<GeneralObjectDefinition> GeneralObjectDefinitions { get; set; }
    }
}
