using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.ServiceModels
{
   public class ObjectFileField : ServiceObjectField
    {
        public string FileName { get; set; }

        public string FileUrl { get; set; }

        public string FileFullPath { get; set; }

        public string FileExtension { get; set; }

        public int FileSize { get; set; }

        public string FileCRC { get; set; }
        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public override bool isFieldValueEqualAs(string v)
        {
            return (string.IsNullOrEmpty(this.FileName) && string.IsNullOrEmpty(v))
                || 
                (!string.IsNullOrEmpty(this.FileName) && this.FileName.Equals(v, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
