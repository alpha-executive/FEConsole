using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.FEConsole.Shared.Services
{
    public interface IFileStorageProvider
    {
        string ProtocolName { get; }
        bool IsSupport(string fileFullName);

        Task<byte[]> DownloadFileAsync(string fileFullName);

        Stream OpenFileStream(string fileFullName);

        Task<string> UploadFileAsync(byte[] content);

        Task<string> UploadFileAsync(Stream stream);

        Task<bool> DeleteFileAsync(string fileFullName);
    }
}
