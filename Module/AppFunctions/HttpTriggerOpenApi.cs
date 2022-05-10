using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Bygdrift.Warehouse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Module.AppFunctions
{

    //God dok: https://www.ais.com/self-documenting-azure-functions-with-c-and-openapi-part-two/

    public class HttpTriggerOpenApi
    {
        public HttpTriggerOpenApi(ILogger<HttpTriggerOpenApi> logger) => App = new AppBase<Settings>(logger);

        public AppBase<Settings> App { get; private set; }

        [FunctionName("payload")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "OS2IOT" })]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Payload([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);

            string responseMessage = "Data recieved: " + requestBody;

            App.Log.LogInformation("aaa");
            App.Log.LogError("bbb");
            App.Log.LogInformation("Data recieved:" + requestBody);
            await App.DataLake.SaveStringAsync(requestBody, "raw", DateTime.Now.ToString("O") + "_payload.txt", Bygdrift.DataLakeTools.FolderStructure.DatePath);

            return new OkObjectResult(responseMessage);
        }
    }
}

