using FE.Creator.FEConsole.Shared.Models;
using FE.Creator.FEConsole.Shared.Services.Cryptography;
using FE.Creator.ObjectRepository;
using FE.Creator.ObjectRepository.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    /// <summary>
    /// API to provide data cryptography services, include:
    ///  GET: /api/Cryptography/GeneratePrivateKeyPairs
    ///       Generate RSA key pairs
    /// POST: /api/Cryptography/EncryptData
    ///       Encrypt data with rsa key.
    ///       parameters: string data to be encrypted.
    /// POST: /api/Cryptography/DecryptData
    ///       Decrypt data with rsa key.
    ///       parameters: data: base64 string data to be decrypted.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CryptographyController : ControllerBase
    {
        ISymmetricCryptographyService cryptoservice = null;
        IRSACryptographyService rsaCryptoService = null;
        IObjectService objectService = null;
        private readonly ILogger logger = null;
        private string GetSystemCryptographKeys()
        {
            logger.LogDebug("Start GetSystemCryptographKeys...");
            var objDefs = objectService.GetAllObjectDefinitions();
            var findObjDef = (from def in objDefs
                              where def.ObjectDefinitionName.Equals("AppConfig", StringComparison.InvariantCultureIgnoreCase)
                              select def).FirstOrDefault();

            var svcObjects = objectService.GetServiceObjects(findObjDef.ObjectDefinitionID, 
                new string[]{ "cryptoSecurityKey" }, 1, 1, null);
            string cryptoKey = svcObjects[0].GetPropertyValue<PrimeObjectField>("cryptoSecurityKey").GetStrongTypeValue<string>();
            logger.LogDebug("get cryptoKey of length　" + cryptoKey.Length);
            logger.LogDebug("End GetSystemCryptographKeys");
            return cryptoKey;
        }

        public CryptographyController(ISymmetricCryptographyService cryptoservice,
            IRSACryptographyService rsaCryptoService,
            IObjectService objectService,
            ILogger logger)
        {
            this.cryptoservice = cryptoservice;
            this.objectService = objectService;
            this.rsaCryptoService = rsaCryptoService;
            this.logger = logger;
        }

        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public HttpResponseMessage GeneratePrivateKeyPairs()
        {
            logger.LogDebug("Start GeneratePrivateKeyPairs");
            HttpResponseMessage result = null;
            byte[] privateKey = rsaCryptoService.getEncryptionKeys();

            if (privateKey != null)
            {
                logger.LogDebug("privateKey is not null");
                result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ByteArrayContent(privateKey);
                result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "rsa.key";
            }
            else
            {
                logger.LogDebug("privateKey is null");
                result = new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            logger.LogDebug("End GeneratePrivateKeyPairs");
            return result;
        }

        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public HttpResponseMessage DownloadCipherMachineApp()
        {
            logger.LogDebug("Start DownloadCipherMachineApp");
            HttpResponseMessage result = null;

            result = new HttpResponseMessage(HttpStatusCode.OK);
            byte[] content = System.IO.File.ReadAllBytes(System.IO.Path.Combine(Environment.CurrentDirectory,
                "App_Data", "FECipherMachine.exe"));
            result.Content = new ByteArrayContent(content);
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = "FECipherMachine.exe";

            logger.LogDebug("End DownloadCipherMachineApp");
            return result;
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<GenericDataModel> EncryptData([FromBody]GenericDataModel data)
        {
            logger.LogDebug("Start EncryptData");
            byte[] bdata = System.Text.UTF8Encoding.UTF8.GetBytes(data.StringData);
            logger.LogDebug("bdata length of " + bdata.Length);

            byte[] sdata = cryptoservice.EncryptData(bdata, GetSystemCryptographKeys());
            logger.LogDebug("sdata length of " + sdata.Length);

            logger.LogDebug("End EncryptData");
            return this.Ok(new GenericDataModel
            {
                StringData = Convert.ToBase64String(sdata)
            });
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<GenericDataModel> DecryptData([FromBody]GenericDataModel data)
        {
            logger.LogDebug("Start DecryptData");
            byte[] bdata = Convert.FromBase64String(data.StringData);
            logger.LogDebug("bdata length of " + bdata.Length);

            byte[] sdata = cryptoservice.DecryptData(bdata, GetSystemCryptographKeys());
            logger.LogDebug("sdata length of " + sdata.Length);

            logger.LogDebug("End DecryptData");
            return this.Ok(new GenericDataModel()
                {
                    StringData = System.Text.UTF8Encoding.UTF8.GetString(sdata)
                });
        }
    }
}
