using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FE.Creator.Admin.Models
{
    public class FileUploadSetting
    {
        public string MaxFileSize { get; set; }
        public string AllowedExtensions { get; set; }
    }
}
