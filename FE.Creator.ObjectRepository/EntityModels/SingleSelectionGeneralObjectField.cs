using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.EntityModels
{
   internal partial class SingleSelectionGeneralObjectField : GeneralObjectField
    {
        public int SelectedItemID { get; set; }
    }
}
