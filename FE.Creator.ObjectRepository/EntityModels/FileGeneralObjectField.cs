using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.ObjectRepository.EntityModels
{
   internal partial class FileGeneralObjectField : GeneralObjectField
    {
        public FileGeneralObjectField()
        {
            this.Created = DateTime.Now;
            this.Updated = DateTime.Now;
        }

        public string FileName { get; set; }

        public string FileUrl { get; set; }

        public string FileFullPath { get; set; }

        public string FileExtension { get; set; }

        public int FileSize { get; set; }

        public string FileCRC { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }
    }
}
