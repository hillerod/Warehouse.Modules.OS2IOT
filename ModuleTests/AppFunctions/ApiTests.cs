using Bygdrift.Warehouse;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.AppFunctions;
using Module.AppFunctions.Models;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleTests.AppFunctions
{
    [TestClass]
    public class ApiTests
    {
        private readonly Mock<ILogger<Api>> loggerMock = new();
        private readonly Api function;
        private readonly AppBase<Settings> app;

        public ApiTests()
        {
            function = new Api(loggerMock.Object);
            app = new AppBase<Settings>();
            app.DataLakeQueue.QueueName = "payloads";
        }

        [TestMethod]
        public async Task GetAndDelete()
        {
            await app.DataLakeQueue.DeleteMessagesAsync();
            var res0 = GetBody(await function.QueuesPeek(default));
            Assert.IsNull(res0);
            await app.DataLakeQueue.AddMessageAsync(JsonConvert.SerializeObject(new { data = "content", date = DateTime.Now }));
            var res1 = GetBody(await function.QueuesPeek(default));
            Assert.AreEqual(1, res1.Count());
            var res2 = GetBody(await function.QueuesGetAndDelete(default));
            Assert.AreEqual(1, res2.Count());
            var res3 = GetBody(await function.QueuesGetAndDelete(default));
            Assert.IsNull(res3);
            var errors = function.App.Log.GetErrorsAndCriticals();
            Assert.AreEqual(0, errors.Count());
        }

        private static IEnumerable<string> GetBody(IActionResult res)
        {
            var okResult = res as OkObjectResult;
            Assert.IsNotNull(okResult);
            return (okResult.Value as IEnumerable<QueueResponse>)?.Select(o => o.Body);
        }

        [TestMethod]
        public async Task AddDataToDataLakeQueue()
        {
            var res = await app.DataLakeQueue.AddMessageAsync(JsonConvert.SerializeObject(new { data = "content", date = DateTime.Now }));
        }

        [TestMethod]
        public async Task PurgeDataToDataLakeQueue()
        {
            await app.DataLakeQueue.DeleteMessagesAsync();
        }

        [TestMethod]
        public async Task DevicesGetAsHtml()
        {
            var g = app.HostName;
            var res = await function.DevicesGetAsHtml(default, "a81758fffe043f5d");

        }
    }
}
