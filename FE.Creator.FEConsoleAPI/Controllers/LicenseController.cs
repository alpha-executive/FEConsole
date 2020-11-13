using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using FE.Creator.ObjectRepository;
using System.Threading.Tasks;
using FE.Creator.ObjectRepository.ServiceModels;
using FE.Creator.ObjectRepository.EntityModels;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using FE.Creator.FEConsole.Shared.Services.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using FE.Creator.FEConsole.Shared.Models;
using FE.Creator.FEConsole.Shared.Services.FileStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Localization;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    public class LicenseController : FEAPIBaseController
    {
        private static string SYS_LANG_CHINESE = "zh-CN";
        private static string SYS_LANG_ENGLISH = "en-US";
        private static string SYS_DEFAULT_DATEFORMAT = "MM/dd/yyyy";
        private static string SYS_DEFAULT_THEME = "skin-blue-light";
        private static int SYS_PULL_PUBLISHER_MESSAGE = 1;
        private static string SYS_PUBLISHER_URL = "http://localhost";
        private static string SYS_EMPTY_LICENSE_FILE = @"<?xml version=""1.0"" encoding=""utf-8""?><license></license>";
        IObjectService objectService = null;
        IFileStorageService storageService = null;
        IRSACryptographyService cryptoGraphysvc = null;
        ISymmetricCryptographyService symmetricCryptoService = null;
        protected IConfiguration configuration;
        private readonly ILogger<LicenseController> logger = null;

        MemoryCache licenseCache = new MemoryCache(new MemoryCacheOptions() {
         SizeLimit = 1024000
        });


        public LicenseController(IObjectService objectService,
            IRSACryptographyService cryptographysvc,
            ISymmetricCryptographyService symmetricCryptoService,
            IFileStorageService storageService,
            IConfiguration configuration,
            ILogger<LicenseController> logger,
            IServiceProvider provider) : base(provider)
        {
            this.objectService = objectService;
            this.cryptoGraphysvc = cryptographysvc;
            this.symmetricCryptoService = symmetricCryptoService;
            this.logger = logger;
            this.storageService = storageService;
            this.configuration = configuration;
        }

        private string ReadResourceContent(string path)
        {
            logger.LogDebug("Start ReadResourceContent");
            logger.LogDebug(string.Format("path: {0}", path));
            string resourceFile = string.Format("resx://{0}/{1}", typeof(LicenseController).Assembly.FullName, path);
            
            using (StreamReader reader = new StreamReader(storageService.OpenFileContentStream(resourceFile)))
            {
                logger.LogDebug("End ReadResourceContent");
                return reader != null ? reader.ReadToEnd() 
                            : string.Empty;
            }
        }

        private int ResolveCategoryGroup(XElement config)
        {
            logger.LogDebug("Start ResolveCategoryGroup");
            string group = config.Attribute("group").Value;
            string groupKey = config.Attribute("key").Value;

            if (!this.objectService.IsObjectDefinitionGroupExists(group))
            {
                logger.LogWarning(string.Format("group {0} is not exists.", group));
                logger.LogDebug("End ResolveCategoryGroup");
                return this.objectService.CreateOrUpdateObjectDefinitionGroup(new ObjectRepository.ServiceModels.ObjectDefinitionGroup
                {
                    GroupKey = groupKey,
                    GroupName = group
                });
            }
            logger.LogDebug("End ResolveCategoryGroup");
            return this.objectService.GetObjectDefinitionGroupByName(group).GroupID;
        }
    

        private ObjectDefinitionField ParseObjectDefinitionField(XElement property)
        {
            string typeString = property.Attribute("type").Value;
            switch (typeString)
            {
                case "String":
                case "Integer":
                case "Long":
                case "Datetime":
                case "Number":
                case "Binary":
                    logger.LogDebug("Parse Binary field.");
                    PrimeDefinitionField field = new PrimeDefinitionField();
                    field.PrimeDataType = (ObjectRepository.EntityModels.PrimeFieldDataType)Enum.Parse(typeof(ObjectRepository.EntityModels.PrimeFieldDataType), typeString);
                    field.ObjectDefinitionFieldName = property.Attribute("name").Value;
                    field.ObjectDefinitionFieldKey = property.Attribute("key").Value;
                    logger.LogDebug(string.Format("{0} = {1}", field.ObjectDefinitionFieldName, field.ObjectDefinitionFieldKey));
                    return field;
                case "File":
                    
                    ObjectDefinitionField fileField = new ObjectDefinitionField(ObjectRepository.EntityModels.GeneralObjectDefinitionFieldType.File);
                    fileField.ObjectDefinitionFieldKey = property.Attribute("key").Value;
                    fileField.ObjectDefinitionFieldName = property.Attribute("name").Value;
                    logger.LogDebug("Parse File field " + fileField.ObjectDefinitionFieldName);

                    return fileField;
                case "ObjRef":
                    logger.LogDebug("Parse ObjRef field.");
                    ObjRefDefinitionField refField = new ObjRefDefinitionField();
                    refField.ObjectDefinitionFieldKey = property.Attribute("key").Value;
                    refField.ObjectDefinitionFieldName = property.Attribute("name").Value;
                    refField.ReferedObjectDefinitionID = this.objectService.GetObjectDefinitionByName(property.Attribute("refName").Value).ObjectDefinitionID;

                    logger.LogDebug(string.Format("{0} = {1}", refField.ObjectDefinitionFieldName, refField.ReferedObjectDefinitionID));

                    return refField;
                case "SingleSelection":
                    SingleSDefinitionField ssField = new SingleSDefinitionField();
                    ssField.ObjectDefinitionFieldKey = property.Attribute("key").Value;
                    ssField.ObjectDefinitionFieldName = property.Attribute("name").Value;
                    
                    foreach(XElement item in property.Descendants("choice"))
                    {
                        ssField.SelectionItems.Add(new DefinitionSelectItem()
                        {
                            SelectDisplayName = item.Attribute("displayName").Value,
                            SelectItemKey = item.Attribute("value").Value
                        });
                    }
                    logger.LogDebug("Parse SingleSelection field : " + ssField.ObjectDefinitionFieldName);
                    return ssField;
                default:
                    break;
            }

            return null;
        }

       private async Task ResolveEntity(int groupId, XElement entity)
        {
            logger.LogDebug("Start ResolveEntity");
            ObjectDefinition definition = new ObjectDefinition();
            
            definition.ObjectDefinitionKey = entity.Attribute("key").Value;
            definition.ObjectDefinitionGroupID = groupId;
            definition.ObjectDefinitionName = entity.Attribute("name").Value;
            var requestUser = await GetLoginUser();
            definition.ObjectOwner = requestUser;
            definition.UpdatedBy = requestUser;

            var currentObjectDefinition = this.objectService.GetObjectDefinitionByName(definition.ObjectDefinitionName);
            definition.IsFeildsUpdateOnly = currentObjectDefinition != null;

            //if there is already a object there, do not register it.
            if (currentObjectDefinition == null)
            {
                logger.LogWarning(string.Format("NOT FOUND {0} in system, create new one", definition.ObjectDefinitionName));
                foreach (XElement prop in entity.Descendants("property"))
                {
                    ObjectDefinitionField objDefField = ParseObjectDefinitionField(prop);
                    if (objDefField != null)
                    {
                        definition.ObjectFields.Add(objDefField);
                    }
                }

                this.objectService.CreateORUpdateObjectDefinition(definition);
            }
            logger.LogDebug("End ResolveEntity");
        }

        private async Task ProcessConfig(string config)
        {
            XDocument configDoc = XDocument.Parse(config);
            XElement configElement = configDoc.Descendants("config").FirstOrDefault();
            if(configElement != null){
                logger.LogInformation("process the system module config file.");
                int groupId = ResolveCategoryGroup(configElement);
                if (groupId > 0)
                {
                    foreach (XElement entity in configElement.Descendants("entity"))
                    {
                        await ResolveEntity(groupId, entity);
                    }
                }
            }
        }

        private bool IsValidLicense(string license)
        {
            try
            {
                logger.LogDebug("Start IsValidLicense");
                XDocument licenseDoc = XDocument.Load(new StringReader(license));
                string grantlist = licenseDoc.Descendants("grantlist").FirstOrDefault().ToString(SaveOptions.DisableFormatting);
                string productKey = licenseDoc.Descendants("productkey").FirstOrDefault().Value;

                logger.LogDebug("productKey = " + productKey);
                byte[] contentToVerified = UTF8Encoding.UTF8.GetBytes(grantlist);
                string publicKey = configuration["SiteSettings:Security:PublicKey"];
                logger.LogDebug("publicKey = " + publicKey);
                bool isValid = cryptoGraphysvc.VerifySignedHash(contentToVerified,
                    Convert.FromBase64String(productKey),
                    publicKey);

                logger.LogWarning(string.Format("Product Key is Valid ? {0}", isValid));

                logger.LogDebug("End IsValidLicense");
                return isValid;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Critical Error in IsValidLicense");
            }

            return false;
        }

        private int GetAppObjectDefintionIdByName(string defName)
        {
            var objDefs = objectService.GetAllObjectDefinitions();
            var findObjDef = (from def in objDefs
                              where def.ObjectDefinitionName.Equals(defName, StringComparison.InvariantCultureIgnoreCase)
                              select def).FirstOrDefault();

            return findObjDef.ObjectDefinitionID;
        }
        private async Task UpdateSystemConfig(string license)
        {
            logger.LogDebug("Start UpdateSystemConfig");
            var requestUser = await GetLoginUser();
            ServiceObject svObject = new ServiceObject();
            svObject.ObjectName = "FE Configuration";
            svObject.ObjectOwner = User.Identity.Name;
            svObject.OnlyUpdateProperties = false;
            svObject.UpdatedBy = requestUser;
            svObject.CreatedBy = requestUser;
            svObject.ObjectDefinitionId = GetAppObjectDefintionIdByName("AppConfig");

            logger.LogDebug(string.Format("svObject.ObjectName = {0}", svObject.ObjectName));
            logger.LogDebug(string.Format("svObject.ObjectDefinitionId = {0}", svObject.ObjectDefinitionId));
            logger.LogDebug(string.Format("svObject.ObjectOwner = {0}", svObject.ObjectOwner));

            var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            // Culture contains the information of the requested culture
            var culture = rqf?.RequestCulture?.Culture?.Name;

            if(culture == null || culture == "")
            {
                culture = System.Threading.Thread.CurrentThread.CurrentUICulture?.Name;
            }

            logger.LogDebug(culture);
            //language
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "language",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.String,
                    Value = culture
                    .ToLower()
                    .Equals(SYS_LANG_CHINESE, StringComparison.InvariantCultureIgnoreCase) ? SYS_LANG_CHINESE : SYS_LANG_ENGLISH
                }
            });

            //dateTimeFormat
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "dateTimeFormat",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.String,
                    Value = SYS_DEFAULT_DATEFORMAT
                }
            });

            //rsa key.
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "cryptoSecurityKey",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.String,
                    Value = Convert.ToBase64String(symmetricCryptoService.getEncryptionKeys())
                }
            });

            //systemTheme.
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "systemTheme",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.String,
                    Value = SYS_DEFAULT_THEME
                }
            });

            //pullMessageFromPublisher.
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "pullMessageFromPublisher",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.Integer,
                    Value = SYS_PULL_PUBLISHER_MESSAGE
                }
            });
            //pullMessagePublisherUrl.
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "pullMessagePublisherUrl",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.String,
                    Value = SYS_PUBLISHER_URL
                }
            });
            //systemVersion.
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "systemVersion",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.String,
                    Value = Assembly.GetExecutingAssembly().GetName().Version.ToString(4)
                }
            });

            //license.
            svObject.Properties.Add(new ObjectKeyValuePair()
            {
                KeyName = "license",
                Value = new PrimeObjectField()
                {
                    PrimeDataType = PrimeFieldDataType.String,
                    Value = license
                }
            });

            objectService.CreateORUpdateGeneralObject(svObject);
            logger.LogDebug("End UpdateSystemConfig");
        }

        private async Task RegistApplication(string license)
        {
            logger.LogDebug("Start RegistApplication");
            string licenseMap = ReadResourceContent("FE.Creator.FEConsoleAPI.Config.Module.LicenseMaps.xml");
            logger.LogDebug(string.Format("licenseMap = {0}", licenseMap));
            XDocument document = XDocument.Parse(licenseMap);
            var files = from f in document.Descendants("module")
                        select new
                        {
                            LicenseId = f.Attribute("licenseId").Value,
                            FileUrl = f.Value
                        };
            
            foreach(var f in files)
            {
                string configureContent = ReadResourceContent(f.FileUrl);
                logger.LogDebug(string.Format("configure content = {0}", configureContent));
                await ProcessConfig(configureContent);
            }

            await UpdateSystemConfig(license);
            logger.LogDebug("End RegistApplication");
        }
        
        private string LoadLicenseFromConfig()
        {
           var objects =  objectService.GetServiceObjects(
                    GetAppObjectDefintionIdByName("AppConfig"),
                    new string[] {"license" },
                    1,
                    1, 
                    null);

            var licenseData = objects
                .FirstOrDefault()
                .GetPropertyValue<PrimeObjectField>("license")
                .GetStrongTypeValue<string>();
            logger.LogDebug(string.Format("license data from DB: {0}", licenseData));
            return IsValidLicense(licenseData)? licenseData : SYS_EMPTY_LICENSE_FILE;
        }

        private XDocument getCachedLicense()
        {
           var license = licenseCache.GetOrCreate<XDocument>("license", entry=>
            {
                entry.SetSlidingExpiration(TimeSpan.FromHours(1));
               
                string licenseContent = LoadLicenseFromConfig();
                var lic = XDocument.Load(new StringReader(licenseContent));

                entry.SetSize(licenseContent.Length);
                logger.LogDebug(String.Format("get license from DB: {0}", lic));

                return lic;
            });
          
            return license;
        }

        private void UpdateLicenseStatus(IEnumerable<LicensedModule> licModules)
        {
            XDocument licenseDoc = getCachedLicense();
            if (licenseDoc != null)
            {
                XElement licenseElement = licenseDoc.Descendants("grantlist").FirstOrDefault();
                var grantedList = from g in licenseDoc.Descendants("moudle")
                                  select g.Value;

                foreach (var lic in licModules)
                {
                    lic.ExpiredDate = DateTime.ParseExact(licenseElement.Attribute("expireddate").Value, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    lic.Licensed = grantedList.Contains(lic.ModuleId);
                }
            }
        }

        [HttpPost]
        [RequestSizeLimit(1024000)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Post(IFormFile licenseFile)
        {
            logger.LogDebug("Start LicenseController.Post");
            if(licenseFile != null 
                && licenseFile.Length > 0)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    await licenseFile.CopyToAsync(stream);
                    string license = System.Text.UTF8Encoding.UTF8.GetString(stream.ToArray());
                    logger.LogDebug(string.Format("license : {0}", license));
                    if (IsValidLicense(license))
                    {
                        await RegistApplication(license);
                    }
                    else
                    {
                        logger.LogError("License file is not valid");
                    }
                }
            }

            logger.LogDebug("End LicenseController.Post");
            return this.Ok();
        }

        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LicensedModule>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LicensedModule>>> RegisterList()
        {
            string licenseMap = ReadResourceContent("FE.Creator.FEConsoleAPI.Config.Module.LicenseMaps.xml");
            XDocument document = XDocument.Parse(licenseMap);
            var files = (from f in document.Descendants("module")
                        select new LicensedModule
                        {
                            ModuleName = f.Attribute("name").Value,
                            ModuleDescription = f.Attribute("Description").Value,
                            ModuleId = f.Attribute("licenseId").Value,
                            Licensed = false,
                            ExpiredDate = DateTime.UtcNow
                        }).ToList();

            UpdateLicenseStatus(files);

            return this.Ok(await Task.FromResult<IEnumerable<LicensedModule>>(files));
        }
    
        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public FileResult DownloadLicense()
        {
            string licenseMap = ReadResourceContent("FE.Creator.FEConsoleAPI.Config.Module.LicenseMaps.xml");
            logger.LogDebug(string.Format("licenseMap = {0}", licenseMap));
            XDocument document = XDocument.Parse(licenseMap);
            var modules = from f in document.Descendants("module")
                        select new XElement("moudle", f.Attribute("licenseId").Value);

            XDocument licenseDocument = new XDocument();

            XElement grandListElement = new XElement("grantlist", modules.ToArray());
            grandListElement.Add(new XAttribute("expireddate", DateTime.Now.AddYears(1).ToString("MM/dd/yyyy")));
            grandListElement.Add(new XAttribute("version", "1.0.0.0"));

            byte[] signedData = cryptoGraphysvc.HashAndSignBytes(Encoding.UTF8.GetBytes(grandListElement.ToString(SaveOptions.DisableFormatting)),
                    configuration["SiteSettings:Security:PrivateKey"]);
            var productKeyElment = new XElement("productkey",
                    Convert.ToBase64String(signedData));

            XElement rootElment = new XElement("license", 
                        grandListElement, 
                        productKeyElment);
            licenseDocument.Add(rootElment);

            string license = licenseDocument.ToString();
            var licenseFile = new FileContentResult(Encoding.UTF8.GetBytes(license)
                , "application/xml");
            licenseFile.FileDownloadName = "license.xml";

            return licenseFile;
        }

    }
}
