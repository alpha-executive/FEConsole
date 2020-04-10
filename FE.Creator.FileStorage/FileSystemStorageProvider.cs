using FE.Creator.FEConsole.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.FileStorage
{
    public class FileSystemStorageProvider : IFileStorageProvider
    {
        private string rootDirectory = null;
        private static readonly string FILE_PROTOCOL = @"file://";
        public FileSystemStorageProvider(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public Task<bool> DeleteFileAsync(string fileFullName)
        {
            FileInfo file = new FileInfo(fileFullName);
            if (file.Exists)
            {
                file.Delete();
            }

            return Task.FromResult(true);
        }

        public async Task<byte[]> DownloadFileAsync(string fileFullName)
        {
            string filePath = ExtractLocalPath(fileFullName);
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
               return await File.ReadAllBytesAsync(filePath);
            }

            throw new FileNotFoundException(fileFullName);
        }

        public bool IsSupport(string fileFullName)
        {
            return fileFullName.StartsWith(FILE_PROTOCOL);
        }

        public Stream OpenFileStream(string fileFullName)
        {
            string filePath = ExtractLocalPath(fileFullName);
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                return File.OpenRead(filePath);
            }

            throw new FileNotFoundException(fileFullName);
        }

        private static string ExtractLocalPath(string fileFullName)
        {
            return fileFullName.Substring(FILE_PROTOCOL.Length);
        }

        public async Task<string> UploadFileAsync(byte[] content)
        {
            string filePath = EnsureFileSavePath();
          
            await File.WriteAllBytesAsync(filePath, content);
            string fileUri = FILE_PROTOCOL + filePath;

            return fileUri;
        }

        private string EnsureFileSavePath()
        {
            var fileName = Path.GetRandomFileName();
            var filePath = Path.Combine(rootDirectory, DateTime.Now.ToString("yyyyMMdd"), fileName);

            FileInfo file = new FileInfo(filePath);
            //ensure the directory is exists.
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }

            return filePath;
        }

        public async Task<string> UploadFileAsync(Stream stream)
        {
            string filePath = EnsureFileSavePath();

            using (var fs = File.OpenWrite(filePath))
            {
              await stream.CopyToAsync(fs);
            }
            string fileUri = FILE_PROTOCOL + filePath;

            return fileUri;
        }
    }
}
