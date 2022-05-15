using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bygdrift.Warehouse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
//using Microsoft.OpenApi.Models;

namespace Module.AppFunctions
{
    //God dok: https://www.ais.com/self-documenting-azure-functions-with-c-and-openapi-part-two/

    public class PostedPayloads
    {
        public PostedPayloads(ILogger<PostedPayloads> logger) => App = new AppBase<Settings>(logger);

        public AppBase<Settings> App { get; private set; }

        [FunctionName("payload")]
        //[OpenApiOperation(operationId: "Run", tags: new[] { "OS2IOT" })]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Payload([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var res = "";
            var headers = req.Headers;

            foreach (var item in headers)
                res += $"Key: {item.Key}, Value: {item.Value}\n";


            if (headers.TryGetValue("Authorization", out var value))
                res += $"header: {value.First()}\n";

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);

            res += "Data recieved: {requestBody}\n";

            App.Log.LogInformation(res);
            await App.DataLake.SaveStringAsync(res, "raw", DateTime.Now.ToString("O") + "_payload_V2.txt", Bygdrift.DataLakeTools.FolderStructure.DatePath);

            return new OkObjectResult(res);
        }
    }
}