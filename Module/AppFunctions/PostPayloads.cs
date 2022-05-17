using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bygdrift.Warehouse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Module.Refines;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using Microsoft.OpenApi.Models;

namespace Module.AppFunctions
{
    //God dok: https://www.ais.com/self-documenting-azure-functions-with-c-and-openapi-part-two/

    public class PostPayloads
    {
        public PostPayloads(ILogger<PostPayloads> logger) => App = new AppBase<Settings>(logger);

        public AppBase<Settings> App { get; private set; }

        [FunctionName(nameof(Payload))]
        //[OpenApiOperation(operationId: "Run", tags: new[] { "OS2IOT" })]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Payload([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            if (!Authorized(req))
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

        private bool Authorized(HttpRequest req)
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
    }
}