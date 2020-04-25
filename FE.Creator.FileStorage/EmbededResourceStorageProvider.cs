using FE.Creator.FEConsole.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FE.Creator.FileStorage
{
    /// <summary>
    /// read the resource with full path like resx://{assembly}/{namespace}.{filename}.{extension}
    /// e.g. resx://fe.creator.filestorage.test.png
    /// </summary>
    public class EmbededResourceStorageProvider : IFileStorageProvider
    {
        private static readonly string FILE_PROTOCOL = @"resx://";


        public string ProtocolName => FILE_PROTOCOL;

        public Task<bool> DeleteFileAsync(string fileFullName)
        {
            throw new NotImplementedException();
        }

        private Stream LoadAssemblyResourceStream(string fileFullName)
        {
            string pattern = "resx://(?<assembly>[^/]+)/(?<resource>.*)";
            Regex regex = new Regex(pattern);
            var match = regex.Match(fileFullName);

            if (match.Success)
            {
                string assemblyFile = match.Groups["assembly"].Value;
                string resource = match.Groups["resource"].Value;

                if(!string.IsNullOrEmpty(assemblyFile)
                    && !string.IsNullOrEmpty(resource))
                {
                    Assembly assembly = Assembly.Load(assemblyFile);
                    var stream = assembly.GetManifestResourceStream(resource);

                    return stream;
                }
            }

            throw new ArgumentException("invalid fileFullName parameter");
        }

        public async Task<byte[]> DownloadFileAsync(string fileFullName)
        {
            using (var stream = LoadAssemblyResourceStream(fileFullName))
            {
                using(var mstream = new MemoryStream())
                {
                    await stream.CopyToAsync(mstream);

                    return mstream.ToArray();
                }
            }
        }

        public bool IsSupport(string fileFullName)
        {
            return fileFullName != null
                      && fileFullName.StartsWith(FILE_PROTOCOL);
        }

        public Stream OpenFileStream(string fileFullName)
        {
            var stream = LoadAssemblyResourceStream(fileFullName);

            return stream;
        }

        public Task<string> UploadFileAsync(byte[] content)
        {
            throw new NotImplementedException();
        }

        public Task<string> UploadFileAsync(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
