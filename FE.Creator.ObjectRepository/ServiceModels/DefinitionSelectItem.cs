using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
   public class DefinitionSelectItem
    {
        public DefinitionSelectItem()
        {
            this.SelectItemID = -1;
        }

        public int  SelectItemID { get; set; }

        public string SelectDisplayName { get; set; }

        public string SelectItemKey { get; set; }
    }
}
