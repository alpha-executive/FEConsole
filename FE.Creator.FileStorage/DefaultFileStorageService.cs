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
        ThumbinalConfig thumbinalConfig = null;
        public DefaultFileStorageService(IFileStorageProvider defaultProvider,
            ThumbinalConfig thumbinalConfig)
        {
            if (defaultProvider == null)
                throw new ArgumentNullException(nameof(defaultProvider));

            this.defaultProvider = defaultProvider;
            this.thumbinalConfig = thumbinalConfig;
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

        public Stream OpenFileContentStream(string fileFullName)
        {
            IFileStorageProvider provider = GetFileStorageProvider(fileFullName);
            if (provider == null)
            {
                throw new ArgumentException(nameof(provider) + " of the file storage service is null");
            }

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

        public async Task<byte[]> DownloadFileContentAsync(string fileFullName)
        {
            IFileStorageProvider provider = GetFileStorageProvider(fileFullName);

            if(provider == null)
            {
                throw new ArgumentException(nameof(provider) + " of the file storage service is null");
            }
            return await provider
                        .DownloadFileAsync(fileFullName);
        }
        private async Task<FileStorageInfo> SaveFileContent(Stream stream, 
            string uniqueFileName, 
            string format, 
            bool createThumbinal,
            string fileFriendlyName)
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
                        thumbinalConfig.Width,
                        thumbinalConfig.Height,
                        fileStream);
                }
            }
            string fileCrc = string.Empty;
            long streamLength = 0;
            using (var fileStream = defaultProvider.OpenFileStream(fileFullName))
            {
                fileCrc = CalculateCRC(fileStream);
                streamLength = stream.CanSeek ? stream.Length
                : fileStream.Length;
            }

            return StoreFileInfo(streamLength,
               uniqueFileName,
               fileCrc,
               fileFullName,
               thumbinalFileName,
               fileFriendlyName);
        }
        private async Task<FileStorageInfo> SaveFileContent(byte[] fileContents,
                string uniqueFileName,
                string format,
                bool createThumbinal,
                string fileFriendlyName)
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
                    thumbinalConfig.Width,
                    thumbinalConfig.Height,
                    fileContents);
            }

            return StoreFileInfo(fileContents.Length,
                uniqueFileName, 
                fileCrc, 
                fileFullName, 
                fileThumbinalFullName,
                fileFriendlyName);
        }

        private FileStorageInfo StoreFileInfo(long fileSize,
            string uniqueFileName,
            string fileCrc, 
            string fileFullName,
            string fileThumbinalFullName,
            string fileFriendlyName)
        {
            StoredFile indexInfo = new StoredFile();
            indexInfo.fileUniqueName = uniqueFileName;
            indexInfo.fileFullName = fileFullName;
            indexInfo.fileCRC = fileCrc;
            indexInfo.fileSize = fileSize;
            indexInfo.fileThumbinalFullName = fileThumbinalFullName;
            indexInfo.fileFriendlyName = fileFriendlyName;

            //indexInfo.fileThumbinalFullName = await defaultProvider.UploadFileAsync();
            saveLocalFileIndex(indexInfo);

            return new FileStorageInfo()
            {
                FileUniqueName = indexInfo.fileUniqueName,
                FileUri = indexInfo.fileFullName,
                Creation = DateTime.Now,
                LastUpdated = DateTime.Now,
                Size = fileSize,
                CRC = fileCrc,
                FileFriendlyName = fileFriendlyName
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

            return OpenFileContentStream(fileFullName);
        }

        public async Task<byte[]> GetFileContentAsync(string uniqueFileName)
        {
            var fileInfo = SearchFileInFileIndex(uniqueFileName);

            if (fileInfo == null)
                throw new FileNotFoundException(uniqueFileName);

            var fileFullName = fileInfo.fileFullName;

            return await DownloadFileContentAsync(fileFullName);
        }

        public async Task<FileStorageInfo> SaveFileAsync(byte[] fileContents, string fileExtension, bool createThumbnial = false, string fileFriendlyName = null)
        {
            string fileName = fileFriendlyName ?? Path.GetRandomFileName();
            FileStorageInfo storageInfo = await SaveFileContent(fileContents, 
                fileName, 
                fileExtension.ToLower(), 
                createThumbnial,
                fileFriendlyName);
            
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

        public async Task<FileStorageInfo> SaveFileAsync(Stream stream, string fileExtension, bool createThumbnial = false, string fileFriendlyName = null)
        {
            string uniqueFileName = Path.GetRandomFileName();
            // string path = Path.Combine(StoreRoot, DateTime.Now.ToString("yyyyMMdd"), fileName);
            FileStorageInfo storageInfo = await SaveFileContent(stream,
                uniqueFileName,
                fileExtension.ToLower(),
                createThumbnial,
                fileFriendlyName);

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
