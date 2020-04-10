using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FE.Creator.FEConsole.Shared.Models.FileStorage
{
    public class FileStorageInfo
    {
        public string FileUniqueName { get; set; }
        public string FileFriendlyName { get; set; }
        public string FileUri { get; set; }

        public DateTime Creation { get; set; }

        public DateTime LastUpdated { get; set; }

        public string CRC { get; set; }

        public long Size { get; set; }
    }
}
