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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using Module.AppFunctions.Models;
using Module.Refines;
using System;
using Module.Services;
using Bygdrift.Tools.CsvTool;

namespace Module.AppFunctions
{
    //https://os2iot-zgvbxkrhecgmo.azurewebsites.net/api/swagger/ui
    public class Api
    {
        public static readonly string BasePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

        public Api(ILogger<Api> logger)
        {
            App = new AppBase<Settings>(logger);
            App.DataLakeQueue.QueueName = "payloads";
            App.CsvConfig.DateFormatKind = FormatKind.TimeOffsetDST;
        }

        public readonly AppBase<Settings> App;

        //God dok: https://www.ais.com/self-documenting-azure-functions-with-c-and-openapi-part-two/
        [OpenApiOperation(operationId: nameof(QueuesAdd), tags: new[] { "Queues" }, Summary = "Used from OS2IOT to parse data. To test this, the OS2IOT_Authorization key has to be set", Visibility = OpenApiVisibilityType.Important)]
        [FunctionName(nameof(QueuesAdd))]
        [OpenApiRequestBody("application/json", typeof(string), Description = "The json that comes from OS2IOT")]
        [OpenApiSecurity(schemeName: "OS2IOT_Authorization", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header, Description = "Used in in a POST API-call from OS2IOT. Has to be special, because OS2IOT has a specific way of authorization. The key comes from OS2IOT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> QueuesAdd([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "queues/Add")] HttpRequest req)
        {
            if (!OS2IOTAuthorized(req))
                return new UnauthorizedResult();

            var json = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(json))
                return new OkResult();

            if (!ValidateJSON(json))
                return new BadRequestObjectResult("Data is not valid json");

            await App.DataLakeQueue.AddMessageAsync(json);
            return new OkResult();
        }

        [FunctionName(nameof(QueuesGetAndDelete))]
        [OpenApiOperation(operationId: nameof(QueuesGetAndDelete), tags: new[] { "Queues" }, Summary = "Get all queues from the server and deletes them", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "amount", In = ParameterLocation.Query, Required = false, Type = typeof(int?), Description = "The amount of fetched messages. Default = 0 means return all", Visibility = OpenApiVisibilityType.Undefined)]
        //[OpenApiParameter(name: "jsonContains", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Looks if json contains the searched pattern. Default = null means return all", Visibility = OpenApiVisibilityType.Undefined)]
        //[OpenApiParameter(name: "deleteQueues", In = ParameterLocation.Query, Required = false, Type = typeof(bool), Description = "Wether the fetched queues should be deleted from the server. Default = false")]
        [OpenApiSecurity(schemeName: "OS2IOT_Authorization", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header, Description = "Used in in a POST API-call from OS2IOT. Has to be special, because OS2IOT has a specific way of authorization. The key comes from OS2IOT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<QueueResponse>), Summary = "successful operation", Description = "successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Description = "No messages found")]
        public async Task<IActionResult> QueuesGetAndDelete([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "queues/GetAndDelete")] HttpRequest req)
        {
            if (!OS2IOTAuthorized(req))
                return new UnauthorizedResult();

            var amount = GetNullableInt(req?.Query["amount"]);
            var queues = (await App.DataLakeQueue.GetMessagesAsync(amount))?.ToList();
            if (queues == null)
                return new OkObjectResult(default);

            var res = new List<QueueResponse>();
            foreach (var queue in queues)
                res.Add(new QueueResponse(queue));

            await App.DataLakeQueue.DeleteMessagesAsync(queues);
            return new OkObjectResult(res);
        }

        [FunctionName(nameof(QueuesGetAsJsonAndDelete))]
        [OpenApiOperation(operationId: nameof(QueuesGetAsJsonAndDelete), tags: new[] { "Queues" }, Summary = "Get all queues from the server formated as json and deletes them", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "amount", In = ParameterLocation.Query, Required = false, Type = typeof(int?), Description = "The amount of fetched messages. Default = 0 means return all", Visibility = OpenApiVisibilityType.Undefined)]
        [OpenApiParameter(name: "groupByProperty", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "You can group data by a propertyname. Can be left blank.", Visibility = OpenApiVisibilityType.Undefined)]
        [OpenApiSecurity(schemeName: "OS2IOT_Authorization", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header, Description = "Used in in a POST API-call from OS2IOT. Has to be special, because OS2IOT has a specific way of authorization. The key comes from OS2IOT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(JArray), Summary = "successful operation", Description = "successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Description = "No messages found")]
        public async Task<IActionResult> QueuesGetAsJsonAndDelete([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "queues/GetAsJsonAndDelete")] HttpRequest req)
        {
            if (!OS2IOTAuthorized(req))
                return new UnauthorizedResult();

            var amount = GetNullableInt(req?.Query["amount"]);
            var groupHeader = req?.Query["groupByProperty"];
            var queues = (await App.DataLakeQueue.GetMessagesAsync(amount))?.ToList();
            if (queues == null)
                return new OkObjectResult(default);

            var json = QueuesRefine.RefineToJson(App, queues, groupHeader);
            await App.DataLakeQueue.DeleteMessagesAsync(queues);
            return new OkObjectResult(json);
        }

        //[FunctionName(nameof(QueuesGetSortedAndDelete))]
        //[OpenApiOperation(operationId: nameof(QueuesGetSortedAndDelete), tags: new[] { "Queues" }, Summary = "Get all queues from the server and deletes them", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(name: "amount", In = ParameterLocation.Query, Required = false, Type = typeof(int?), Description = "The amount of fetched messages. Default = 0 means return all", Visibility = OpenApiVisibilityType.Undefined)]
        //[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(List<string>), Description = "A json array of strings that this API shall try to search for. Data will be returned en this given order. If empty, all will be returned.", Required = false)]
        //[OpenApiSecurity(schemeName: "OS2IOT_Authorization", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header, Description = "Used in in a POST API-call from OS2IOT. Has to be special, because OS2IOT has a specific way of authorization. The key comes from OS2IOT")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<QueueResponse>), Summary = "successful operation", Description = "successful operation")]
        //[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Description = "No messages found")]
        //public async Task<IActionResult> QueuesGetSortedAndDelete([HttpTrigger(AuthorizationLevel.Function, "post", Route = "queues/GetSortedAndDelete")] HttpRequest req)
        //{
        //    if (!OS2IOTAuthorized(req))
        //        return new UnauthorizedResult();

        //    var amount = GetNullableInt(req?.Query["amount"]);
        //    var queues = (await App.DataLakeQueue.GetMessagesAsync(amount))?.ToList();
        //    var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    var res = new List<QueueResponse>();
        //    var tasks = new List<Task>();

        //    if (queues == null)
        //        return new OkObjectResult(default);

        //    if (string.IsNullOrEmpty(requestBody))
        //    {
        //        foreach (var queue in queues)
        //        {
        //            res.Add(new QueueResponse(queue));
        //            tasks.Add(App.DataLakeQueue.DeleteMessageAsync(queue));
        //        }
        //    }
        //    else
        //    {
        //        List<string> array;
        //        try
        //        {
        //            array = JsonConvert.DeserializeObject<List<string>>(requestBody);
        //        }
        //        catch (System.Exception e)
        //        {
        //            return new BadRequestErrorMessageResult("The body is not correct formed like: \"[\\\"Besked 4\\\", \\\"Besked 2\\\"]\".");
        //        }

        //        foreach (var item in array)
        //            foreach (var queue in queues.Where(o => o.Body.ToString().Contains(item, System.StringComparison.InvariantCulture)))
        //            {
        //                res.Add(new QueueResponse(queue));
        //                tasks.Add(App.DataLakeQueue.DeleteMessageAsync(queue));
        //            }
        //    }

        //    await Task.WhenAll(tasks);
        //    return new OkObjectResult(res);
        //}

        [FunctionName(nameof(QueuesPeek))]
        [OpenApiOperation(operationId: nameof(QueuesPeek), tags: new[] { "Queues" }, Summary = "Get up to 32 queues from the server without deleting them. It is not possible to return more with peek when working with queues.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "amount", In = ParameterLocation.Query, Required = false, Type = typeof(int?), Description = "The amount of fetched messages. Default = 0 means return up to 32", Visibility = OpenApiVisibilityType.Undefined)]
        //[OpenApiParameter(name: "jsonContains", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Looks if json contains the searched pattern. Default = null means return all", Visibility = OpenApiVisibilityType.Undefined)]
        //[OpenApiParameter(name: "deleteQueues", In = ParameterLocation.Query, Required = false, Type = typeof(bool), Description = "Wether the fetched queues should be deleted from the server. Default = false")]
        [OpenApiSecurity(schemeName: "OS2IOT_Authorization", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header, Description = "Used in in a POST API-call from OS2IOT. Has to be special, because OS2IOT has a specific way of authorization. The key comes from OS2IOT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<QueueResponse>), Summary = "successful operation", Description = "successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Description = "No messages found")]
        public async Task<IActionResult> QueuesPeek([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "queues/peek")] HttpRequest req)
        {
            if (!OS2IOTAuthorized(req))
                return new UnauthorizedResult();

            var res = new List<QueueResponse>();
            var amount = GetNullableInt(req?.Query["amount"]);
            var queues = (await App.DataLakeQueue.PeekMessagesAsync(amount));
            if (queues == null)
                return new OkObjectResult(default);

            foreach (var queue in queues)
                res.Add(new QueueResponse(queue));

            return new OkObjectResult(res);
        }

        //https://os2iot-zgvbxkrhecgmo.azurewebsites.net/api/swagger/ui#/Tests/QueuesAddTest
        [FunctionName(nameof(QueuesAddTest))]
        [OpenApiOperation(operationId: nameof(QueuesAddTest), tags: new[] { "Queues/Tests" }, Summary = "Add a test queue", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "message", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "A test on content. By default = 'Test'")]
        [OpenApiSecurity(schemeName: "OS2IOT_Authorization", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header, Description = "Used in in a POST API-call from OS2IOT. Has to be special, because OS2IOT has a specific way of authorization. The key comes from OS2IOT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Azure.Storage.Queues.Models.SendReceipt), Summary = "successful operation", Description = "successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "No modules found")]
        public async Task<IActionResult> QueuesAddTest([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "queues/addTest")] HttpRequest req)
        {
            var azure_root = Environment.GetEnvironmentVariable("HOME") + @"\site\wwwroot";

            var dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var dllParentPath = Path.GetDirectoryName(dllPath);
                return new OkObjectResult($"azure:{azure_root}, path: {dllPath}, parent: {dllParentPath}");

            


            //if (!OS2IOTAuthorized(req))
            //    return new UnauthorizedResult();

            //var message = req?.Query["message"];
            //if (string.IsNullOrEmpty(message))
            //    message = "Test";

            //var receipt = await App.DataLakeQueue.AddMessageAsync(message);
            //if (receipt != null)
            //    return new OkObjectResult(receipt);
            //else
            //    return new InternalServerErrorResult();
        }

        [FunctionName(nameof(QueuesTest))]
        [OpenApiOperation(operationId: nameof(QueuesTest), tags: new[] { "Queues/Tests" }, Summary = "Hello world", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "testPath", In = ParameterLocation.Path, Required = false, Type = typeof(string), Description = "A test")]
        [OpenApiParameter(name: "testQuery", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "A test")]
        [OpenApiSecurity(schemeName: "OS2IOT_Authorization", SecuritySchemeType.ApiKey, Name = "Authorization", In = OpenApiSecurityLocationType.Header, Description = "Used in in a POST API-call from OS2IOT. Has to be special, because OS2IOT has a specific way of authorization. The key comes from OS2IOT")]
        [OpenApiRequestBody("text/plain", typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "No modules found")]
        public async Task<IActionResult> QueuesTest([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "queues/test/{testPath}")] HttpRequest req, string testPath)
        {
            if (!OS2IOTAuthorized(req))
                return new UnauthorizedResult();

            var testQuery = req?.Query["testQuery"];
            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync();
            return new OkObjectResult($"testPath: {testPath}, testQuery: {testQuery}, Body: {body}.");
        }


        [FunctionName(nameof(DevicesGetAsHtml))]
        [OpenApiOperation(operationId: nameof(DevicesGetAsHtml), tags: new[] { "devices" }, Summary = "Gives information about a given device as html. Designed to be used in conjunction with scanning a qr-code created with the api: devices/getAsQR.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "deveui", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The device EUI from OS2IOT of this specific device", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/html", bodyType: typeof(string), Summary = "successful operation", Description = "successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Description = "No messages found")]
        public async Task<ActionResult> DevicesGetAsHtml([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "q/{deveui}")] HttpRequest req, string deveui)
        {
            if (string.IsNullOrEmpty(deveui))
                return new BadRequestObjectResult("deveui skal være sat");

            var service = new OS2IOTApiService(App);
            var applications = await service.GetApplicationsAsync();
            var device = applications?.data.SelectMany(o => o.iotDevices)?.SingleOrDefault(p => p.deviceEUI.Equals(deveui, StringComparison.OrdinalIgnoreCase));
            if (device == null)
                return new BadRequestObjectResult("Kunne ikke finde device");

            var application = applications?.data.SingleOrDefault(o => o.iotDevices.Select(p => p.deviceEUI).Any(q => q.Equals(deveui, StringComparison.OrdinalIgnoreCase)));
            if (application == null)
            {
                App.Log.LogCritical("Hvis der er et device så skal der også være en application");
                return new BadRequestObjectResult("Der er en intern fejl");
            }

            var html = File.ReadAllText("./AppFunctions/Helpers/DevicesGetAsHtmlResponse.html");
            html = html.Replace("{{Owner}}", WebUtility.HtmlEncode(App.Settings.Owner));
            foreach (var prop in application.GetType().GetProperties())
            {
                var data = WebUtility.HtmlEncode(prop.GetValue(application, null)?.ToString());
                html = html.Replace("{{App." + prop.Name + "}}", data);
            }

            foreach (var prop in device.GetType().GetProperties())
            {
                var data = WebUtility.HtmlEncode(prop.GetValue(device, null)?.ToString());
                html = html.Replace("{{Dev." + prop.Name + "}}", data);
            }

            return new ContentResult { Content = html, ContentType = "text/html" };
        }

        [FunctionName(nameof(DevicesGetAsQR))]
        [OpenApiOperation(operationId: nameof(DevicesGetAsQR), tags: new[] { "devices" }, Summary = "Creates QR-codes that can be applied every device in an application", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "applicationName", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The applicationName from OS2IOT of this specific application", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "deveuis", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Comma seperated list of devicesEUIs that should be created", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/html", bodyType: typeof(string), Summary = "successful operation", Description = "successful operation")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Description = "No messages found")]
        public async Task<ActionResult> DevicesGetAsQR([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "devices/getAsQR")] HttpRequest req)
        {
            var euis = new List<string>();
            var applicationName = req?.Query["applicationName"];
            if (!string.IsNullOrEmpty(applicationName))
            {
                var service = new OS2IOTApiService(App);
                var applications = await service.GetApplicationsAsync();
                var application = applications?.data.FirstOrDefault(o => o.name.ToLower() == applicationName.ToString().ToLower());
                if (application == null)
                    return new BadRequestObjectResult($"There are no application with name {applicationName}.");

                foreach (var device in application.iotDevices)
                    euis.Add(device.deviceEUI);
            }

            var deveuis = req?.Query["deveuis"];
            if (!string.IsNullOrEmpty(deveuis))
                euis.AddRange(deveuis.ToString().Split(','));

            var owner = WebUtility.HtmlEncode(App.Settings.Owner);
            var li = "";
            foreach (var eui in euis)
                li += $"<li><h1>{owner}</h1><div class=\"qrcode\" qr=\"{App.HostName}/api/devices/getAsHtml?deveui={eui}\"></div><h2>Device EUI: {eui}</h2></li>\n";

            var html = File.ReadAllText("./AppFunctions/Helpers/DevicesGetAsQR.html");
            html = html.Replace("{{li}}", li);
            return new ContentResult { Content = html, ContentType = "text/html" };
        }

        private bool OS2IOTAuthorized(HttpRequest req)
        {
            if (req.Headers.TryGetValue("Authorization", out var value))
            {
                var authorization = value.FirstOrDefault()?.Replace("Bearer", string.Empty).Trim();
                return authorization == App.Settings.OS2IOTAuthorization;
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