using FE.Creator.FEConsole.Shared.Models.FileStorage;
using FE.Creator.FEConsole.Shared.Services;
using FE.Creator.FEConsole.Shared.Services.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FE.Creator.FileStorage
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class DefaultFileStorageService : IFileStorageService
    {
        private Dictionary<Type, IFileStorageProvider> storageProviders = new Dictionary<Type, IFileStorageProvider>();
        IFileStorageProvider defaultProvider = null;
        public DefaultFileStorageService(IFileStorageProvider defaultProvider)
        {
            if (defaultProvider == null)
                throw new ArgumentNullException(nameof(defaultProvider));

            this.defaultProvider = defaultProvider;
            AddStorageProvider(defaultProvider);
            EnsureDBCreated();
        }

        private void EnsureDBCreated()
        {
            using (SqliteLocalFileIndexDBContext dbContext = new SqliteLocalFileIndexDBContext())
            {
                dbContext.Database.EnsureCreated();
            }
        }

        private StoredFile SearchFileInFileIndex(string fileUniqueName)
        {
            using (SqliteLocalFileIndexDBContext dbContext = new SqliteLocalFileIndexDBContext())
            {
                try
                {
                    var fileInfo = (from f in dbContext.Files
                                    where f.fileUniqueName.Equals(fileUniqueName)
                                    select f).FirstOrDefault();

                    if (fileInfo != null)
                    {
                        return fileInfo;
                    }
                }
                catch(Exception e)
                {
                    throw e;
                }
            }

            return null;
        }


        private void saveLocalFileIndex(StoredFile indexInfo)
        {
            if (indexInfo == null || string.IsNullOrEmpty(indexInfo.fileFullName))
            {
                throw new ArgumentNullException("indexInfo or indexInfo.fileFullName is null");
            }

            using (SqliteLocalFileIndexDBContext dbContext = new SqliteLocalFileIndexDBContext())
            {
                try
                {
                    dbContext.Files.Add(indexInfo);
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private Stream OpenFileContent(string fileFullName)
        {
            IFileStorageProvider provider = GetFileStorageProvider(fileFullName);
            return provider
                        .OpenFileStream(fileFullName);
        }

        private IFileStorageProvider GetFileStorageProvider(string fileFullName)
        {
            if (storageProviders == null || storageProviders.Count == 0)
            {
                throw new ArgumentException(nameof(storageProviders) + "is null or empty");
            }

            var provider = (from p in storageProviders.Values
                            where p.IsSupport(fileFullName)
                            select p).FirstOrDefault();
            return provider;
        }

        private async Task<byte[]> DownloadFileContentAsync(string fileFullName)
        {
            IFileStorageProvider provider = GetFileStorageProvider(fileFullName);
            return await provider
                        .DownloadFileAsync(fileFullName);
        }
        private async Task<FileStorageInfo> SaveFileContent(Stream stream, string uniqueFileName, string format, bool createThumbinal)
        {
            if (defaultProvider == null)
                throw new ArgumentException(nameof(defaultProvider) + " is null");

            string fileFullName = await defaultProvider.UploadFileAsync(stream);
            string thumbinalFileName = string.Empty;
            if (createThumbinal)
            {
                using (var fileStream = defaultProvider.OpenFileStream(fileFullName))
                {
                    thumbinalFileName = await SimpleFileThumbinalGenerator.CreateThumbnail(defaultProvider,
                        format,
                        260,
                        260,
                        fileStream);
                }
            }
            string fileCrc = string.Empty;
            using (var fileStream = defaultProvider.OpenFileStream(fileFullName))
            {
                fileCrc = CalculateCRC(fileStream);
            }

            return StoreFileInfo(stream.Length,
               uniqueFileName,
               fileCrc,
               fileFullName,
               thumbinalFileName);
        }
        private async Task<FileStorageInfo> SaveFileContent(byte[] fileContents,
                string uniqueFileName,
                string format,
                bool createThumbinal)
        {
            if (defaultProvider == null)
                throw new ArgumentException(nameof(defaultProvider) + " is null");

            string fileCrc = CalculateCRC(fileContents);
            string fileFullName = await defaultProvider.UploadFileAsync(fileContents);
            string fileThumbinalFullName = string.Empty;
            if (createThumbinal)
            {
                fileThumbinalFullName = await SimpleFileThumbinalGenerator.CreateThumbnail(defaultProvider,
                    format,
                    260,
                    260,
                    fileContents);
            }

            return StoreFileInfo(fileContents.Length,
                uniqueFileName, 
                fileCrc, 
                fileFullName, 
                fileThumbinalFullName);
        }

        private FileStorageInfo StoreFileInfo(long fileSize, string uniqueFileName, string fileCrc, string fileFullName, string fileThumbinalFullName)
        {
            StoredFile indexInfo = new StoredFile();
            indexInfo.fileUniqueName = uniqueFileName;
            indexInfo.fileFullName = fileFullName;
            indexInfo.fileCRC = fileCrc;
            indexInfo.fileSize = fileSize;
            indexInfo.fileThumbinalFullName = fileThumbinalFullName;

            //indexInfo.fileThumbinalFullName = await defaultProvider.UploadFileAsync();
            saveLocalFileIndex(indexInfo);

            return new FileStorageInfo()
            {
                FileUniqueName = indexInfo.fileUniqueName,
                FileUri = indexInfo.fileFullName,
                Creation = DateTime.Now,
                LastUpdated = DateTime.Now,
                Size = fileSize,
                CRC = fileCrc
            };
        }

        private string CalculateCRC(byte[] fileContents)
        {
            using (var sha = SHA256.Create())
            {
                byte[] checksum = sha.ComputeHash(fileContents);
              
                return Convert.ToBase64String(checksum);
            }
        }
        private string CalculateCRC(Stream streamContent)
        {
            using(var sha = SHA256.Create())
            {
                byte[] checksum = sha.ComputeHash(streamContent);

                return Convert.ToBase64String(checksum);
            }
        }

        public void AddStorageProvider(IFileStorageProvider provider)
        {
            if (!storageProviders.ContainsKey(provider.GetType()))
            {
                storageProviders[provider.GetType()] = provider;
            }
            else
            {
                storageProviders.Add(provider.GetType(), provider);
            }
        }

        public async Task<byte[]> getFileContent(string uniqueFileName)
        {
            var bytes = await GetFileContentAsync(uniqueFileName);

            return bytes;
        }
        public Stream GetFileContentStream(string uniqueFileName)
        {
            var fileInfo = SearchFileInFileIndex(uniqueFileName);

            if (fileInfo == null)
                throw new FileNotFoundException(uniqueFileName);

            var fileFullName = fileInfo.fileFullName;

            return OpenFileContent(fileFullName);
        }

        public async Task<byte[]> GetFileContentAsync(string uniqueFileName)
        {
            var fileInfo = SearchFileInFileIndex(uniqueFileName);

            if (fileInfo == null)
                throw new FileNotFoundException(uniqueFileName);

            var fileFullName = fileInfo.fileFullName;

            return await DownloadFileContentAsync(fileFullName);
        }

        public async Task<FileStorageInfo> SaveFileAsync(byte[] fileContents, string fileExtension, bool createThumbnial = false)
        {
            string fileName = Path.GetRandomFileName();
            // string path = Path.Combine(StoreRoot, DateTime.Now.ToString("yyyyMMdd"), fileName);
            FileStorageInfo storageInfo = await SaveFileContent(fileContents, 
                fileName, 
                fileExtension.ToLower(), 
                createThumbnial);
            
            return storageInfo;
        }

        public async void DeleteFile(string uniqueFileName)
        {
            await defaultProvider.DeleteFileAsync(uniqueFileName);
        }

        public async Task<byte[]> GetFileThumbinalAsync(string uniqueFileName)
        {
            var fileInfo = SearchFileInFileIndex(uniqueFileName);

            if (fileInfo == null)
                throw new FileNotFoundException(uniqueFileName);

            var fileFullName = fileInfo.fileThumbinalFullName;

            return await DownloadFileContentAsync(fileFullName);
        }

        public async Task<FileStorageInfo> SaveFileAsync(Stream stream, string fileExtension, bool createThumbnial = false)
        {
            string fileName = Path.GetRandomFileName();
            // string path = Path.Combine(StoreRoot, DateTime.Now.ToString("yyyyMMdd"), fileName);
            FileStorageInfo storageInfo = await SaveFileContent(stream,
                fileName,
                fileExtension.ToLower(),
                createThumbnial);

            return storageInfo;
        }

        public FileStorageInfo GetStoredFileInfo(string uniqueFileName)
        {
            var fileInfo = SearchFileInFileIndex(uniqueFileName);

            if (fileInfo == null)
                throw new FileNotFoundException(uniqueFileName);


            return new FileStorageInfo()
            {
                FileUniqueName = fileInfo.fileUniqueName,
                FileFriendlyName = fileInfo.fileFriendlyName,
                FileUri = fileInfo.fileFullName,
                CRC = fileInfo.fileCRC,
                Size = fileInfo.fileSize
            };
        }
    }
}
