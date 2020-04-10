﻿using FE.Creator.FEConsole.Shared.Models.FileStorage;
using System.IO;
using System.Threading.Tasks;

namespace FE.Creator.FEConsole.Shared.Services.FileStorage
{
   public interface IFileStorageService
    {
        Task<FileStorageInfo> SaveFileAsync(byte[] fileContents, string fileExtension, bool createThumbnial = false);

        Task<FileStorageInfo> SaveFileAsync(Stream stream, string fileExtension, bool createThumbnial = false);

        Stream GetFileContentStream(string uniqueFileName);

        Task<byte[]> GetFileContentAsync(string uniqueFileName);

        Task<byte[]> GetFileThumbinalAsync(string uniqueFileName);

        FileStorageInfo GetStoredFileInfo(string uniqueFileName);

        void DeleteFile(string uniqueFileName);
    }
}
