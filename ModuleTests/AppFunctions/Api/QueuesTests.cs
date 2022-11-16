using Bygdrift.Tools.DataLakeTool;
using Bygdrift.Warehouse;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.AppFunctions;
using Module.AppFunctions.Helpers.Models;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleTests.AppFunctions.Api
{
    [TestClass]
    public class QueuesTests
    {
        private readonly Mock<ILogger<ApiQueues>> loggerMock = new();
        private readonly ApiQueues function;
        private readonly AppBase<Settings> app;

        public QueuesTests()
        {
            function = new ApiQueues(loggerMock.Object);
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
    }
}
