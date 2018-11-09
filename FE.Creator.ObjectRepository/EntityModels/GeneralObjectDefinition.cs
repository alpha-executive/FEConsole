using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.EntityModels
{
   internal class GeneralObjectDefinition
    {
        public GeneralObjectDefinition()
        {
            this.GeneralObjectDefinitionFields = new HashSet<GeneralObjectDefinitionField>();
            this.GeneralObjects = new HashSet<GeneralObject>();
        }
        public int GeneralObjectDefinitionID { get; set; }

        public String GeneralObjectDefinitionName { get; set; }

        public String GeneralObjectDefinitionKey { get; set; }

        public int GeneralObjectDefinitionGroupID { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime Updated { get; set; }

        public DateTime Created { get; set; }

        public string ObjectOwner { get; set; }

        public string UpdatedBy { get; set; }

        public virtual GeneralObjectDefinitionGroup GeneralObjectDefinitionGroup { get; set; }

        public virtual ICollection<GeneralObjectDefinitionField> GeneralObjectDefinitionFields { get; set; }

        /// <summary>
        /// the objects created based on current object type.
        /// </summary>
        public virtual ICollection<GeneralObject> GeneralObjects { get; set; }
    }
}
