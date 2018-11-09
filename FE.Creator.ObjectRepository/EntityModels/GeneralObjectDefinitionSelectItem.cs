using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.EntityModels
{
    internal class GeneralObjectDefinitionSelectItem
    {
        public int GeneralObjectDefinitionSelectItemID { get; set; }

        public string SelectDisplayName { get; set; }

        public string SelectItemKey { get; set; }

        public int GeneralObjectDefinitionFieldID { get; set; }

        public SingleSelectionDefinitionField SingleSelectionDefinitionField { get; set; }
    }
}
