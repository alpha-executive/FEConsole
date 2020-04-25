using FE.Creator.ObjectRepository;
using FE.Creator.ObjectRepository.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FE.Creator.FEConsoleAPI.ApiControllers
{
    /// <summary>
    /// Provide the report data to dashboard and other public interfaces.
    /// GET: /api/report/TargetStatusReport
    ///     get the target and task completion statistics report.
    ///     return: array of double, three elements:
    ///         0: Average Target Complete Progress.
    ///         1: Average task complete progress.
    ///  GET: /api/report/YOYObjectUsageReport/{objectNameList}
    ///      get the YOY Object usage report.
    ///      objectNameList: object definition name collections, splited by ",".
    ///      return: array of int, 12 elements, from the month of last year to the current month, each of the value indicate the 
    ///      count of specific objects in that given month.
    /// </summary>
    public class ReportController : FEAPIBaseController
    {
        IObjectService objectService = null;
        ILogger<ReportController> logger = null;
        readonly IConfiguration _configuration;
        public ReportController(IObjectService objectService,
            IConfiguration configuration,
            ILogger<ReportController> logger,
            IServiceProvider provider) : base(provider)
        {
            this.objectService = objectService;
            this.logger = logger;
            this._configuration = configuration;
        }

        private int FindObjectDefinitionIdByName(string defName)
        {
            var objDefs = objectService.GetAllObjectDefinitions();
            var findObjDef = (from def in objDefs
                              where def.ObjectDefinitionName.Equals(defName, StringComparison.InvariantCultureIgnoreCase)
                              select def).FirstOrDefault();

            return findObjDef.ObjectDefinitionID;
        }
        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(double[]), StatusCodes.Status200OK)]
        public async Task<ActionResult<double[]>> TargetStatusReport()
        {
            logger.LogDebug("Start TargetStatusReport");
            int targetDefId = FindObjectDefinitionIdByName("Target");
            var requestUser = await GetLoginUser();
            var targetList = objectService.GetAllSerivceObjects(targetDefId, 
                new string[] { "targetStatus" },
                new ServiceRequestContext()
                {
                    IsDataCurrentUserOnly = bool.Parse(_configuration["SiteSettings:IsDataForLoginUserOnly"]),
                    RequestUser = requestUser,
                    UserSenstiveForSharedData = false
                });

            var targetPercentage = targetList.Count > 0 ? (from t in targetList select t)
                                                            .Average(s => 
                                                                    s.GetPropertyValue<PrimeObjectField>("targetStatus")
                                                                        .GetStrongTypeValue<int>()) 
                                                        : 0;

            //var targetCompletePercentage = targetList.Count > 0 ? (from t in targetList
            //                                                       select t).Average(s =>
            //                                                       s.GetPropertyValue<PrimeObjectField>("targetStatus")
            //                                                             .GetStrongTypeValue<int>() >= 100 ? 100 : 0) 
            //                                                    : 0;

            int taskDefId = FindObjectDefinitionIdByName("Task");
            var taskList = objectService.GetAllSerivceObjects(taskDefId, 
                        new string[] { "taskStatus" },
                        new ServiceRequestContext()
                        {
                            IsDataCurrentUserOnly = bool.Parse(_configuration["SiteSettings:IsDataForLoginUserOnly"]),
                            RequestUser = requestUser,
                            UserSenstiveForSharedData = false
                        });

            var taskPercentage = taskList.Count > 0 ? (from t in taskList select t)
                                                        .Average(s => s.GetPropertyValue<PrimeObjectField>("taskStatus")
                                                                        .GetStrongTypeValue<int>())
                                                    : 0;
            logger.LogDebug("taskPercentage : " + taskPercentage);
            logger.LogDebug("End TargetStatusReport");
            return this.Ok(new double[] { targetPercentage,
                taskPercentage });                                            
        }
        [Route("[action]/{Id}")]
        [HttpGet]
        [ProducesResponseType(typeof(List<int[]>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<int[]>>> YOYObjectUsageReport(string Id)
        {
            logger.LogDebug("Start YOYObjectUsageReport");
            List<int[]> seriesData = new List<int[]>();
            if (!string.IsNullOrEmpty(Id))
            {
                string[] objNames = Id.Split(new char[] { ',' });

                foreach (string objName in objNames)
                {
                    int[] data = await GetYoYStatisticReportData(objName);
                    seriesData.Add(data);
                }
            }

            logger.LogDebug("End YOYObjectUsageReport");
            return this.Ok(seriesData);
        }

        private async Task<int[]> GetYoYStatisticReportData(string objectName)
        {
            int objDefId = FindObjectDefinitionIdByName(objectName);
            var objList = objectService.GetAllSerivceObjects(objDefId, 
                null,
                new ServiceRequestContext()
                {
                    IsDataCurrentUserOnly = bool.Parse(_configuration["SiteSettings:IsDataForLoginUserOnly"]),
                    RequestUser = await GetLoginUser(),
                    UserSenstiveForSharedData = false
                });

            DateTime dateOfLastYear = DateTime.Now.AddYears(-1).AddMonths(1);
            //only take care of the data of recent years.
            var groupInfo = (from o in objList
                             where o.Created >= dateOfLastYear
                             group o.ObjectID by o.Created.ToString("MM-yy") into g
                             select new KeyValuePair<string, int>(g.Key, g.Count())).ToDictionary(ks => ks.Key);

            int[] data = new int[12];
            int idx = 0;
            for (; dateOfLastYear.Date <= DateTime.Now.Date; dateOfLastYear = dateOfLastYear.AddMonths(1))
            {
                var currentMonth = dateOfLastYear.ToString("MM-yy");
                data[idx++] = groupInfo != null && groupInfo.ContainsKey(currentMonth)
                                        ? groupInfo[currentMonth].Value : 0;
            }

            return data;
        }
    }
}
