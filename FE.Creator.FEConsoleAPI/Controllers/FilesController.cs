﻿using FE.Creator.FEConsole.Shared.Models;
using FE.Creator.FEConsole.Shared.Models.FileStorage;
using FE.Creator.FEConsole.Shared.Services.FileStorage;
using FE.Creator.FEConsoleAPI.MVCExtension;
using FE.Creator.FEConsoleAPI.Utilities;
using FE.Creator.ObjectRepository.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    /// <summary>
    ///  GET: /api/Files/DownloadFile/{id}/{parameter}/
    ///      {id}: required: file id
    ///      {parameter}: optional, file name
    ///      return: downloaded file
    ///  POST: api/Files
    ///       upload a file, request must contains multipart/form-data.
    ///  DELETE: api/Files/{id}
    ///       {id}: required string id.
    ///       delete a file with given file {id}. 
    /// </summary>
    public class FilesController : FEAPIBaseController
    {
        private static readonly FormOptions _defaultFormOptions = new FormOptions();
        IFileStorageService storageService = null;
        ILogger<FilesController> logger = null;
        IConfiguration mConfiguration;

        private readonly string[] _permittedExtensions = { ".txt" };

        public FilesController(IFileStorageService storageService,
            IConfiguration _configuration,
            ILogger<FilesController> logger,
            IServiceProvider provider) : base(provider)
        {
            this.storageService = storageService;
            this.mConfiguration = _configuration;
            this.logger = logger;

            List<string> permittedExts = new List<string>();
            mConfiguration.Bind("SiteSettings:FileUpload:PermittedExtension", permittedExts);
            this._permittedExtensions = permittedExts.ToArray();
        }

        private string getContentType(string fileName)
        {
            string mimeType = "application/octet-stream";
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out mimeType);

            if (string.IsNullOrEmpty(mimeType))
            {
                mimeType = "application/octet-stream";
            }

            return mimeType;
            //return "application/octet-stream";
        }
        private string GetFileFriendlyName(string uniqueFileName, string usrProvidedFName)
        {
            if (string.IsNullOrEmpty(usrProvidedFName))
            {
                var finfo = storageService.GetStoredFileInfo(uniqueFileName);
                return finfo != null ? finfo.FileFriendlyName
                        : usrProvidedFName;
            }

            return !string.IsNullOrEmpty(usrProvidedFName) ? HttpUtility.UrlEncode(usrProvidedFName)
                : usrProvidedFName;
        }
        //private string GetFileFormat(string fileFriendlyName)
        //{
        //    return string.IsNullOrEmpty(fileFriendlyName) ? "unknown" 
        //        : Path.GetExtension(fileFriendlyName);
        //}
        /// <summary>
        /// GET /api/Files/DownloadFile/{0}/{1}/
        /// </summary>
        /// <param name="uniqueFileName"></param>
        /// <param name="fileDisplayName"></param>
        /// <param name="thumbinal"></param>
        /// <returns></returns>
        [Route("[action]/{uniqueFileName}/{fileDisplayName?}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // GET: api/Files
        public async Task<FileResult> DownloadFile(string uniqueFileName, string fileDisplayName = null, bool thumbinal = false)
        {
            logger.LogDebug("Start DownloadFile");
            FileResult result = null;

            string fileFriendlyName = GetFileFriendlyName(uniqueFileName, fileDisplayName);
            if (thumbinal)
            {
                logger.LogDebug("get thumbinal image");
                result = await this.GetFileThumbinal(uniqueFileName, fileFriendlyName);
            }
            else
            {
                logger.LogDebug("get original file content");
                result = GetStreamFileContent(uniqueFileName, fileDisplayName);
            }
            
            logger.LogDebug("End DownloadFile");
            return result;
        }
        [Route("[action]/{uniqueFileName}/{fileDisplayName?}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public FileResult DownloadStreamingFile(string uniqueFileName, string fileDisplayName=null)
        {
            return GetStreamFileContent(uniqueFileName,
                fileDisplayName);
        }

        [Route("[action]/{uniqueFileName}/{fileDisplayName?}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<FileResult> DownloadContentFile(string uniqueFileName, string fileDisplayName = null)
        {
            return await GetFileContent(uniqueFileName,
                fileDisplayName);
        }

        private FileResult GetStreamFileContent(string fileUniqueName, string fileDisplayName)
        {
            string fileFriendlyName = GetFileFriendlyName(fileUniqueName, fileDisplayName);
            var stream = storageService.GetFileContentStream(fileUniqueName);

            return File(stream, getContentType(fileFriendlyName),
               fileFriendlyName ?? "download.bin");
        }

        private async Task<FileResult> GetFileContent(string fileUniqueName, string fileDisplayName)
        {
            logger.LogDebug("Start GetFileContent");
            FileResult result = null;
            byte[] content = await storageService.GetFileContentAsync(fileUniqueName);
            if(content != null)
            {
                logger.LogDebug("content found on the server storage.");
                // Serve the file to the client
                string fileName = string.IsNullOrEmpty(fileDisplayName) ? fileUniqueName : fileDisplayName;
                result = new FileContentResult(content, getContentType(fileName));
                result.FileDownloadName = fileName;
            }
            else
            {
                logger.LogDebug("content was not found");
            }

            logger.LogDebug("End GetFileContent");
            return result;
        }

        private async Task<FileResult> GetFileThumbinal(string fileUniqueName, string fileDisplayName = null)
        {
            logger.LogDebug("Start GetFileThumbinal");
            FileResult result = null;
            byte[] content = await storageService.GetFileThumbinalAsync(fileUniqueName);

            if(content != null)
            {
                logger.LogDebug("found content on server storage.");
                //string format = GetFileFormat(fileDisplayName);
                string fileName = string.IsNullOrEmpty(fileDisplayName) ? "file_thumb.bin"
                                                : Path.GetFileNameWithoutExtension(fileDisplayName) + ".bin";
                result = new FileContentResult(content, getContentType(fileName));
                result.FileDownloadName = fileName;
            }

            logger.LogDebug("End GetFileThumbinal");
            return result;
        }

        private Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader =
                MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in 
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }

        // POST: api/Files
        [HttpPost]
        [DisableRequestSizeLimit]
        [DisableFormValueModelBinding]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post(bool thumbinal = false, bool forContent = false)
        {
            logger.LogDebug("Start FileController Post");
            // Check if the request contains multipart/form-data. 
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                logger.LogError($"The request couldn't be processed (Error 1).");
                return BadRequest($"The request couldn't be processed (Error 1).");
            }

            try
            {
                var boundary = MultipartRequestHelper.GetBoundary(
                        MediaTypeHeaderValue.Parse(Request.ContentType),
                                _defaultFormOptions.MultipartBoundaryLengthLimit);
                var reader = new MultipartReader(boundary, HttpContext.Request.Body);

                List<ObjectFileField> files = new List<ObjectFileField>();
                var section = await reader.ReadNextSectionAsync();
                while (section != null)
                {
                    var hasContentDispositionHeader =
                        ContentDispositionHeaderValue.TryParse(
                            section.ContentDisposition, out var contentDisposition);

                    if (hasContentDispositionHeader)
                    {
                        if (MultipartRequestHelper
                            .HasFileContentDisposition(contentDisposition))
                        {
                            // Don't trust the file name sent by the client. To display
                            // the file name, HTML-encode the value.
                            var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                    contentDisposition.FileName.Value);

                            logger.LogInformation("the raw file name is : " + trustedFileNameForDisplay);

                            var extension = Path.GetExtension(contentDisposition.FileName.Value);
                            long fileSizeLimit = long.Parse(mConfiguration["SiteSettings:FileUpload:FileSizeLimit"]);
                            FileStorageInfo info =
                                await storageService.SaveFileAsync(section.Body, extension, thumbinal, trustedFileNameForDisplay);

                            logger.LogDebug("file save path : " + info.FileUri);
                            //FileHelper.ProcessStreamedFile(section, contentDisposition,
                            //     _permittedExtensions, fileSizeLimit);
                           
                            files.Add(new ObjectFileField()
                            {
                                FileName = trustedFileNameForDisplay,
                                //FileFullPath = info.FileUri,

                                FileUrl = string.Format("/api/Files/DownloadFile/{0}/{1}", info.FileUniqueName, WebUtility.UrlEncode(trustedFileNameForDisplay)),
                                FileExtension = extension,
                                Updated = info.LastUpdated,
                                Created = info.Creation,
                                FileCRC = info.CRC,
                                FileSize = (int)(info.Size / 1024),
                            });


                            //only care about single document.
                            if (forContent)
                                break;
                        }

                        // Drain any remaining section body that hasn't been consumed and
                        // read the headers for the next section.
                        section = await reader.ReadNextSectionAsync();
                    }
                }

                //return the action result.
                ActionResult result = forContent ? this.Ok(new UploadFile() { uploaded = 1, fileName = files[0].FileName, url = files[0].FileUrl })
                            : this.Ok(new { status = "success", files = files });

                logger.LogDebug("End FileController Post");
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [Route("{fileUniqueName}")]
        [HttpDelete]
        // DELETE: api/Files/5
        public void DeleteFile(string fileUniqueName)
        {
            logger.LogDebug("Start DeleteFile with id : " + fileUniqueName);

            storageService.DeleteFile(fileUniqueName);

            logger.LogDebug("End DeleteFile");
        }
    }
}
