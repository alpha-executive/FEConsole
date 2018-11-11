using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
    public class SingleSelectionField : ServiceObjectField
    {
        public int SelectedItemID { get; set; }

        public override bool isFieldValueEqualAs(string v)
        {
            return SelectedItemID.ToString().Equals(v, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
