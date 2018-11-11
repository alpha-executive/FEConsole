using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
   public class SingleSDefinitionField : ObjectDefinitionField
    {
        public SingleSDefinitionField() : base(EntityModels.GeneralObjectDefinitionFieldType.SingleSelection) { }

        private List<DefinitionSelectItem> m_SelectionItems = new List<DefinitionSelectItem>();
        public List<DefinitionSelectItem> SelectionItems
        {
            get
            {
                return m_SelectionItems;
            }
        }
    }
}
