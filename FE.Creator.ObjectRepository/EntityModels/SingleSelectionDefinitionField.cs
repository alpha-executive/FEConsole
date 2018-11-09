using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.EntityModels
{
    internal class SingleSelectionDefinitionField:GeneralObjectDefinitionField
    {
        public SingleSelectionDefinitionField()
        {
            this.SelectionItems = new HashSet<GeneralObjectDefinitionSelectItem>();
        }

        /// <summary>
        /// Define the static select items if GeneralObjectDefinitionFieldType = SingleSelection or MultipleSelection
        /// </summary>
        public virtual ICollection<GeneralObjectDefinitionSelectItem> SelectionItems { get; set; }
    }
}
