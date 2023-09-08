using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Bygdrift.Warehouse;
using Module.Services.OS2IOTModels;
using Module.Services;
using System.Linq;

namespace Module.AppFunctions
{
    public class OS2IOTApi
    {
        public readonly AppBase<Settings> App;
        private OS2IOTApiService service;

        public OS2IOTApi(ILogger<ApiQueues> logger)
        {
            App = new AppBase<Settings>(logger);
            App.DataLakeQueue.QueueName = "payloads";
            service = new OS2IOTApiService(App);
        }

        [FunctionName(nameof(GetApplications))]
        [OpenApiOperation(operationId: nameof(GetApplications), tags: new[] { "OS2IOT" }, Summary = "Get all applications from the OS2IOT", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity(schemeName: "OS2IOT_Authorization", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header, Description = "Used in in a POST API-call from OS2IOT. Has to be special, because OS2IOT has a specific way of authorization. The key comes from OS2IOT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Applications), Summary = "successful operation", Description = "successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "No modules found")]
        public async Task<IActionResult> GetApplications([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "os2iot/GetApplications")] HttpRequest req)
        {
            if (!OS2IOTAuthorized(req))
                return new UnauthorizedResult();

            var res = await service.GetApplicationsAsync();
            return new OkObjectResult(res);
        }

        //[FunctionName(nameof(GetDeviceModels))]
        //[OpenApiOperation(operationId: nameof(GetDeviceModels), tags: new[] { "OS2IOT" }, Summary = "Get all devicemodels from the OS2IOT", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiSecurity("Azure Authorization", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query, Description = "A function app key from Azure")]  //https://devkimchi.com/2021/10/06/securing-azure-function-endpoints-via-openapi-auth/
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DeviceModels), Summary = "successful operation", Description = "successful operation")]
        //[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "No modules found")]
        //public async Task<IActionResult> GetDeviceModels([HttpTrigger(AuthorizationLevel.Function, "get", Route = "os2iot/GetDeviceModels")] HttpRequest req)
        //{
        //    var res = await service.GetDeviceModelsAsync();
        //    return new OkObjectResult(res);
        //}

        //[FunctionName(nameof(GetIOTDevices))]
        //[OpenApiOperation(operationId: nameof(GetIOTDevices), tags: new[] { "OS2IOT" }, Summary = "Get all IOTDevices from the OS2IOT", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiSecurity("Azure Authorization", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query, Description = "A function app key from Azure")]  //https://devkimchi.com/2021/10/06/securing-azure-function-endpoints-via-openapi-auth/
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IotDevice[]), Summary = "successful operation", Description = "successful operation")]
        //[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "No modules found")]
        //public async Task<IActionResult> GetIOTDevices([HttpTrigger(AuthorizationLevel.Function, "get", Route = "os2iot/GetIOTDevices")] HttpRequest req)
        //{
        //    var applications = await service.GetApplicationsAsync();
        //    if (applications == null)
        //        return default;

        //    var iotDevices = await service.GetIOTDevicesAsync(applications);
        //    return new OkObjectResult(iotDevices);
        //}

        bool OS2IOTAuthorized(HttpRequest req)
        {
            if (req.Headers.TryGetValue("Authorization", out var value))
            {
                var authorization = value.FirstOrDefault()?.Replace("Bearer", string.Empty).Trim();
                return authorization == App.Settings.OS2IOTAuthorization;
            }
            return false;
        }
    }
}