using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FE.Creator.Admin.Models
{
    public class FileUploadSetting
    {
        public string MaxFileSize { get; set; }

        public string MaxImageSize { get; set; }
        public string AllowedImageExtensions { get; set; }

        public string MaxAttachmentSize { get; set; }
        public string MaxBookSize { get; set; }
        public string AllowedBookExtensions { get; set; }
    }
}
