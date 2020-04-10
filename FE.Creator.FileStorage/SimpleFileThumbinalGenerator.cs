using FE.Creator.FEConsole.Shared.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.FileStorage
{
    interface IThumbinalGenerator
    {
        bool IsMatchFormat(string format);

        Task<string> GetThumbinal(IFileStorageProvider storageProvider, int width, int height, string format, byte[] content);

        Task<string> GetThumbinal(IFileStorageProvider storageProvider, int width, int height, string format, Stream contentStream);
    }
    class GeneralFileThumbinalGenerator : IThumbinalGenerator
    {
        private string mFullFileName = string.Empty;
        private static Dictionary<string, string> thumbinalMappings = new Dictionary<string, string>();
        private static string currentDir = Environment.CurrentDirectory;
        static string ComposeFilePath(string folder, string imageFile)
        {
            return string.Format("file://{0}", Path.Combine(currentDir, folder, imageFile));
        }
        static GeneralFileThumbinalGenerator()
        {
            thumbinalMappings.Add(".docx", ComposeFilePath("localresources", "docx.png"));
            thumbinalMappings.Add(".doc", ComposeFilePath("localresources", "docx.png"));
            thumbinalMappings.Add(".xlsx", ComposeFilePath("localresources", "xlsx.png"));
            thumbinalMappings.Add(".xls", ComposeFilePath("localresources", "xlsx.png"));
            thumbinalMappings.Add(".pptx", ComposeFilePath("localresources", "pptx.png"));
            thumbinalMappings.Add(".ppt", ComposeFilePath("localresources", "pptx.png"));
            thumbinalMappings.Add(".pdf", ComposeFilePath("localresources", "pdf.png"));
            thumbinalMappings.Add(".iso", ComposeFilePath("localresources", "iso.png"));
            thumbinalMappings.Add(".html", ComposeFilePath("localresources", "html.png"));
            thumbinalMappings.Add(".htm", ComposeFilePath("localresources", "html.png"));
            thumbinalMappings.Add(".exe", ComposeFilePath("localresources", "cmd.png"));

            //Zip, rar, 7z, gz, tar.
            thumbinalMappings.Add(".zip", ComposeFilePath("localresources", "zip.png"));
            thumbinalMappings.Add(".rar", ComposeFilePath("localresources", "zip.png"));
            thumbinalMappings.Add(".7z", ComposeFilePath("localresources", "zip.png"));
            thumbinalMappings.Add(".gz", ComposeFilePath("localresources", "zip.png"));
            thumbinalMappings.Add(".tar", ComposeFilePath("localresources", "zip.png"));
        }

        public bool IsMatchFormat(string format)
        {
            return true;
        }

        public GeneralFileThumbinalGenerator()
        { }

        public Task<string> GetThumbinal(IFileStorageProvider storageProvider,
            int width,
            int height,
            string format,
            byte[] imageData)
        {
            return GenerateThumbinal(format);
        }

        private static Task<string> GenerateThumbinal(string format)
        {
            return Task.Run<string>(() =>
            {
                if (thumbinalMappings
                   .ContainsKey(format.ToLower()))
                    return thumbinalMappings[format.ToLower()];

                return ComposeFilePath("localresources", "doc.png");
            });
        }

        public  Task<string> GetThumbinal(IFileStorageProvider storageProvider, int width, int height, string format, Stream contentStream)
        {
            return GenerateThumbinal(format);
        }
    }

    class ImageFileThumbinalGenerator : IThumbinalGenerator
    {
        private string mFormat = string.Empty;
        private static string[] SUPPORTED_FILE_TYPES = new string[]
        {
            ".jpg",".png", ".bmp", ".gif", ".jpeg"
        };

        public bool IsMatchFormat(string format)
        {
            return SUPPORTED_FILE_TYPES.Contains(format.ToLower());
        }
        public ImageFileThumbinalGenerator()
        {}

        public async Task<string> GetThumbinal(IFileStorageProvider storageProvider,
            int width,
            int height,
            string format,
            byte[] imageData)
        {
            string thumbinalPath = string.Empty;
            IImageFormat imageFormat = null;
            using (Image image = Image.Load(imageData, out imageFormat))
            {
                thumbinalPath = await GenerateThumbinal(storageProvider, width, height, imageFormat, image);
            }

            return thumbinalPath;
        }

        private static async Task<string> GenerateThumbinal(IFileStorageProvider storageProvider, int width, int height, IImageFormat imageFormat, Image image)
        {
            string thumbinalPath = string.Empty;
            double ratio = width * 1.0 / image.Width <= height * 1.0 / image.Height ?
                            width * 1.0 / image.Width : height * 1.0 / image.Height;

            var encoder = image.GetConfiguration().ImageFormatsManager.FindEncoder(imageFormat);
            if (encoder is null)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"No encoder was found for image format '{imageFormat.Name}'. Registered encoders include:");
                throw new NotSupportedException(sb.ToString());
            }

            using (MemoryStream ms = new MemoryStream())
            {
                image.Mutate(x => x
                 .Resize((int)(image.Width * ratio), (int)(image.Height * ratio))
                 .Grayscale());

                image.Save(ms, encoder);
                thumbinalPath = await storageProvider.UploadFileAsync(ms.ToArray());
            }

            return thumbinalPath;
        }

        public async Task<string> GetThumbinal(IFileStorageProvider storageProvider, int width, int height, string format, Stream contentStream)
        {
            IImageFormat imageFormat = null;
            string thumbinalPath = string.Empty;
            using (Image image = Image.Load(contentStream, out imageFormat))
            {
                thumbinalPath = await GenerateThumbinal(storageProvider, width, height, imageFormat, image);
            }

            return thumbinalPath;
        }
    }
    public class SimpleFileThumbinalGenerator
    {
        private static IThumbinalGenerator []thumbinalGenerators = new IThumbinalGenerator[]
        {
            new ImageFileThumbinalGenerator(),
            new GeneralFileThumbinalGenerator()
        };

        public static async Task<string> CreateThumbnail(IFileStorageProvider storageProvider,
            string format,
            int width,
            int height,
            byte[] content)
        {

            foreach(var generator in thumbinalGenerators)
            {
                if (generator.IsMatchFormat(format))
                {
                    return await generator.GetThumbinal(storageProvider, 
                        width, 
                        height, 
                        format, 
                        content);
                }
            }

            return string.Empty;
        }

        public static async Task<string> CreateThumbnail(IFileStorageProvider storageProvider,
           string format,
           int width,
           int height,
           Stream contentStream)
        {

            foreach (var generator in thumbinalGenerators)
            {
                if (generator.IsMatchFormat(format))
                {
                    return await generator.GetThumbinal(storageProvider,
                        width,
                        height,
                        format,
                        contentStream);
                }
            }

            return string.Empty;
        }
    }
}
