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
using System.Collections.Generic;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using Module.AppFunctions.Helpers.Models;

namespace Module.AppFunctions.API
{
    public class QueuesApi
    {
        public QueuesApi(ILogger<QueuesApi> logger)
        {
            App = new AppBase<Settings>(logger);
            App.DataLakeQueue.QueueName = "payloads";
        }

        public readonly AppBase<Settings> App;

        [FunctionName(nameof(Get))]
        [OpenApiOperation(operationId: nameof(Get), tags: new[] { "Queues" }, Summary = "Get all queues from the server", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "amount", In = ParameterLocation.Query, Required = false, Type = typeof(int?), Description = "The amount of fetched messages. Default = 0 means return all", Visibility = OpenApiVisibilityType.Undefined)]
        [OpenApiParameter(name: "deleteQueues", In = ParameterLocation.Query, Required = false, Type = typeof(bool), Description = "Wether the fetched queues should be deleted from the server. Default = false")]
        [OpenApiSecurity("Azure Authorization", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query, Description = "A function app key from Azure")]  //https://devkimchi.com/2021/10/06/securing-azure-function-endpoints-via-openapi-auth/
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<QueueResponse>), Summary = "successful operation", Description = "successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "No modules found")]
        public async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = "queues/Get")] HttpRequest req)
        {
            var amount = GetNullableInt(req?.Query["amount"]);
            var queues = (await App.DataLakeQueue.GetMessagesAsync(amount))?.ToList();

            if (queues == null)
                return new OkObjectResult(default);

            var res = new List<QueueResponse>();
            foreach (var queue in queues)
                res.Add(new QueueResponse(queue));

            if (req?.Query["deleteQueues"].ToString().ToLower() == "true")
                await App.DataLakeQueue.DeleteMessagesAsync(queues);

            return new OkObjectResult(res);
        }

        //God dok: https://www.ais.com/self-documenting-azure-functions-with-c-and-openapi-part-two/
        [FunctionName(nameof(Add))]
        [OpenApiOperation(operationId: nameof(Add), tags: new[] { "Queues" }, Summary = "Used from OS2IOT to parse data", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity(schemeName: "OS2IOT_Authorization", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header, Description = "Used in in a POST API-call from OS2IOT. Has to be special, because OS2IOT has a specific way of authorization. The key comes from OS2IOT" )]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Add([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "queues/Add")] HttpRequest req)
        {
            if (!OS2IOTAuthorized(req))
                return new UnauthorizedResult();

            var json = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(json))
                return new OkResult();

            if (!ValidateJSON(json))
                return new BadRequestObjectResult("Data is not valid json");

            App.DataLakeQueue.QueueName = "payloads";
            await App.DataLakeQueue.AddMessageAsync(json);
            return new OkResult();
        }

        [FunctionName(nameof(AddTestQueue))]
        [OpenApiOperation(operationId: nameof(AddTestQueue), tags: new[] { "Tests" }, Summary = "Add a test queue", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "message", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "A test on content. By default = 'Test'")]
        [OpenApiSecurity("Azure Authorization", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query, Description = "A function app key from Azure")]  //https://devkimchi.com/2021/10/06/securing-azure-function-endpoints-via-openapi-auth/
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Azure.Storage.Queues.Models.SendReceipt), Summary = "successful operation", Description = "successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "No modules found")]
        public async Task<IActionResult> AddTestQueue([HttpTrigger(AuthorizationLevel.Function, "get", Route = "queues/addTestQueue")] HttpRequest req)
        {
            var message = req?.Query["message"];
            if (string.IsNullOrEmpty(message))
                message = "Test";

            var receipt = await App.DataLakeQueue.AddMessageAsync(message);
            if (receipt != null)
                return new OkObjectResult(receipt);
            else
                return new InternalServerErrorResult();
        }

        [FunctionName(nameof(Test))]
        [OpenApiOperation(operationId: nameof(Test), tags: new[] { "Tests" }, Summary = "Hello world", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "testPath", In = ParameterLocation.Path, Required = false, Type = typeof(string), Summary = "A test")]
        [OpenApiParameter(name: "testQuery", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "A test")]
        [OpenApiSecurity("Azure Authorization", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query, Description = "A function app key from Azure")]  //https://devkimchi.com/2021/10/06/securing-azure-function-endpoints-via-openapi-auth/
        [OpenApiRequestBody("text/plain", typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "No modules found")]
        public async Task<IActionResult> Test([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "queues/test/{testPath}")] HttpRequest req, string testPath)
        {
            var testQuery = req?.Query["testQuery"];

            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync();

            return new OkObjectResult($"testPath: {testPath}, testQuery: {testQuery}, Body: {body}.");
        }

        private bool OS2IOTAuthorized(HttpRequest req)
        {
            if (req.Headers.TryGetValue("Authorization", out var value))
            {
                var authorization = value.FirstOrDefault()?.Replace("Bearer", string.Empty).Trim();
                return authorization == App.Settings.PostPayloadsAuthorizationKey;
            }
            return false;
        }

        private static bool ValidateJSON(string json)
        {
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        private int? GetNullableInt(string data)
        {
            if (int.TryParse(data, out int value))
                return value;
            return null;
        }
    }
}