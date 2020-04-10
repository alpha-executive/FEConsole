using FE.Creator.FEConsole.Shared.Services.FileStorage;
using FE.Creator.ObjectRepository;
using FE.Creator.ObjectRepository.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharedObjectsController : ControllerBase
    {
        public IObjectService objectService = null;
        IFileStorageService storageService = null;
        public readonly ILogger<SharedObjectsController> logger = null;
        public SharedObjectsController(IObjectService objectService, 
                IFileStorageService storageService,
                ILogger<SharedObjectsController> logger)
        {
            this.objectService = objectService;
            this.storageService = storageService;
            this.logger = logger;
        }

        private List<ServiceObject> GetSharedServiceObjects(int objDefID, 
            string[] properties,
            string shareFieldName,
            int page, 
            int pageSize)
        {
            var svcObjLists = objectService.GetServiceObjects(
                objDefID,
                properties,
                page,
                pageSize,
                null);

            var sharedObjects = (from s in svcObjLists
                                   where s.GetPropertyValue<PrimeObjectField>(shareFieldName)
                                            .GetStrongTypeValue<int>() == 1
                                   select s
                                  )
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();


            return sharedObjects;
        }


        private ObjectDefinition FindObjectDefinitionByName(string defname)
        {
            logger.LogDebug("Start FindObjectDefinitionByName");

            var objDefs = objectService.GetAllObjectDefinitions();

            logger.LogDebug("objDefs.count : " + objDefs.Count);

            var findObjDef = (from def in objDefs
                              where def.ObjectDefinitionName.Equals(defname, StringComparison.InvariantCultureIgnoreCase)
                              select def).FirstOrDefault();

            logger.LogDebug("End FindObjectDefinitionByName");
            return findObjDef;
        }


        private async Task<HttpResponseMessage> DownloadSharedObjectFile(string Id, 
            string filePropertyName, 
            string shareFieldName,
            bool isThumbinal)
        {
            byte[] content = null;

            ServiceObject svo = objectService.GetServiceObjectById(int.Parse(Id),
                new string[] { filePropertyName, shareFieldName },
                null);

            if(svo != null && 
                svo.GetPropertyValue<PrimeObjectField>(shareFieldName)
                                            .GetStrongTypeValue<int>() == 1)
            {
               string fileFullPath = svo.GetPropertyValue<ObjectFileField>(filePropertyName)
                    .FileFullPath;

                if (isThumbinal)
                {
                    content = await storageService
                        .GetFileThumbinalAsync(fileFullPath);
                }
                else
                {
                    content = await storageService
                        .GetFileContentAsync(fileFullPath);
                }
            }

            HttpResponseMessage message = CreateResponseMessage(content,
                svo != null ? svo.GetPropertyValue<ObjectFileField>(filePropertyName)
                .FileName : string.Empty);

            return message;
        }


        private HttpResponseMessage CreateResponseMessage(byte[] content, string fileName)
        {
            logger.LogDebug("Start CreateResponseMessage");
            HttpResponseMessage result = null;

            if (content != null)
            {
                logger.LogDebug("content found on the server storage.");
                // Serve the file to the client
                result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ByteArrayContent(content);
                result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = fileName;
            }
            else
            {
                logger.LogDebug("content was not found");
                result = new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            logger.LogDebug("End GetFileContent");
            return result;
        }

        /// <summary>
        /// For File Download : /api/SharedObjects/DownloadSharedBook/{objectid}
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Route("[action]/{Id}")]
        [HttpGet]
        [ProducesResponseType(typeof(HttpResponseMessage), StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> DownloadSharedDocument(string Id)
        {
            return await DownloadSharedObjectFile(Id,
                "documentFile",
                "documentSharedLevel",
                false);
        }

        /// <summary>
        /// For thumbinal download : /api/SharedObjects/DownloadSharedBook/{objectid}?thumbinal=true
        /// For File Download : /api/SharedObjects/DownloadSharedBook/{objectid}
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="thumbinal"></param>
        /// <returns></returns>
        [Route("[action]/{Id}")]
        [HttpGet]
        [ProducesResponseType(typeof(HttpResponseMessage), StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> DownloadSharedBook(string Id, bool thumbinal = false)
        {
            return await DownloadSharedObjectFile(Id,
                "bookFile", 
                "bookSharedLevel", 
                thumbinal);
        }

        /// <summary>
        /// For thumbinal download : /api/SharedObjects/DownloadSharedImage/{objectid}?thumbinal=true
        /// For File Download : /api/SharedObjects/DownloadSharedImage/{objectid}
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="thumbinal"></param>
        /// <returns></returns>
        [Route("[action]/{Id}")]
        [HttpGet]
        [ProducesResponseType(typeof(HttpResponseMessage), StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> DownloadSharedImage(string Id, bool thumbinal = false)
        {
            return await DownloadSharedObjectFile(Id,
                "imageFile",
                "imageSharedLevel",
                thumbinal);
        }

        /// <summary>
        ///  For File Download : /api/SharedObjects/DownloadArticleImage/{objectid}
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Route("[action]/{Id}")]
        [HttpGet]
        [ProducesResponseType(typeof(HttpResponseMessage), StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> DownloadArticleImage(string Id)
        {
            return await DownloadSharedObjectFile(Id,
                "articleImage",
                "articleSharedLevel",
                 false);
        }

        /// <summary>
        /// /api/custom/SharedObjects/SharedArticles
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ServiceObject>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ServiceObject>> SharedArticles(int page = 1, int pagesize = int.MaxValue)
        {
            var articleDef = FindObjectDefinitionByName("Article");
            int currPage = page > 0 ? page : 1;
            int currPageSize = pagesize > 0 ? pagesize : int.MaxValue;

            var sharedObjects = GetSharedServiceObjects(
                    articleDef.ObjectDefinitionID,
                    new string[] {
                        "articleDesc",
                        "isOriginal",
                        "articleImage",
                        "articleGroup",
                        "articleSharedLevel"
                    },
                    "articleSharedLevel",
                    currPage > 0 ? currPage : 1,
                    currPageSize > 0 ? currPageSize : int.MaxValue
                );

            return this.Ok(sharedObjects);
        }

        /// <summary>
        /// /api/custom/SharedObjects/SharedImages
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ServiceObject>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ServiceObject>> SharedImages(int page = 1, int pagesize = int.MaxValue)
        {
            var imageDef = FindObjectDefinitionByName("Photos");
            int currPage = page > 0 ? page : 1;
            int currPageSize = pagesize > 0 ? pagesize : int.MaxValue;

            var sharedObjects = GetSharedServiceObjects(
                 imageDef.ObjectDefinitionID,
                 new string[] {
                        "imageFile",
                        "imageDesc",
                        "imageCategory",
                        "imageSharedLevel"
                 },
                 "imageSharedLevel",
                 currPage > 0 ? currPage : 1,
                 currPageSize > 0 ? currPageSize : int.MaxValue
             );

            return this.Ok(sharedObjects);
        }

        /// <summary>
        /// /api/custom/SharedObjects/SharedBooks
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ServiceObject>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ServiceObject>> SharedBooks(int page = 1, int pagesize = int.MaxValue)
        {
            var imageDef = FindObjectDefinitionByName("Books");
            int currPage = page > 0 ? page : 1;
            int currPageSize = pagesize > 0 ? pagesize : int.MaxValue;

            var sharedObjects = GetSharedServiceObjects(
                 imageDef.ObjectDefinitionID,
                 new string[] {
                        "bookFile",
                        "bookDesc",
                        "bookAuthor",
                        "bookVersion",
                        "bookSharedLevel",
                        "bookCategory",
                        "bookISBN"
                 },
                 "bookSharedLevel",
                 currPage > 0 ? currPage : 1,
                 currPageSize > 0 ? currPageSize : int.MaxValue
             );

            return this.Ok(sharedObjects);
        }


        /// <summary>
        /// /api/custom/SharedObjects/SharedDocuments
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ServiceObject>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ServiceObject>> SharedDocuments(int page = 1, int pagesize = int.MaxValue)
        {
            var imageDef = FindObjectDefinitionByName("Documents");
            int currPage = page > 0 ? page : 1;
            int currPageSize = pagesize > 0 ? pagesize : int.MaxValue;

            var sharedObjects = GetSharedServiceObjects(
                 imageDef.ObjectDefinitionID,
                 new string[] {
                        "documentFile",
                        "documentSharedLevel"
                 },
                 "documentSharedLevel",
                 currPage > 0 ? currPage : 1,
                 currPageSize > 0 ? currPageSize : int.MaxValue
             );

            return this.Ok(sharedObjects);
        }

        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public int GetSharedImageCount()
        {
            var imageDef = FindObjectDefinitionByName("Photos");

            var images = GetSharedServiceObjects(
                 imageDef.ObjectDefinitionID,
                 new string[] { "imageSharedLevel" },
                 "imageSharedLevel",
                 1,
                 int.MaxValue
                );

            return images.Count;
        }

        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public int GetSharedBookCount()
        {
            var imageDef = FindObjectDefinitionByName("Books");

            var sharedObjects = GetSharedServiceObjects(
               imageDef.ObjectDefinitionID,
               new string[] {
                        "bookSharedLevel"
               },
               "bookSharedLevel",
               1,
               int.MaxValue
           );

            return sharedObjects.Count;
        }

        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public int GetSharedArticleCount()
        {
            var articleDef = FindObjectDefinitionByName("Article");

            var sharedObjects = GetSharedServiceObjects(
                   articleDef.ObjectDefinitionID,
                   new string[] {
                        "articleSharedLevel"
                   },
                   "articleSharedLevel",
                   1,
                   int.MaxValue
               );

            return sharedObjects.Count;
        }

        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public int GetSharedDocumentCount()
        {
            var articleDef = FindObjectDefinitionByName("Documents");

            var sharedObjects = GetSharedServiceObjects(
                   articleDef.ObjectDefinitionID,
                   new string[] {
                        "documentSharedLevel"
                   },
                   "documentSharedLevel",
                   1,
                   int.MaxValue
               );

            return sharedObjects.Count;
        }
    }
}
