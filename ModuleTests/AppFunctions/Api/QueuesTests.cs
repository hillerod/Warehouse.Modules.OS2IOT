﻿using Bygdrift.Tools.DataLakeTool;
using Bygdrift.Warehouse;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.AppFunctions.API;
using Module.AppFunctions.Helpers.Models;
using Module.AppFunctions.OS2IOT;
using Moq;
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
        private readonly Mock<ILogger<QueuesApi>> loggerMock = new();
        private readonly QueuesApi function;
        private readonly AppBase<Settings> app;

        public QueuesTests()
        {
            function = new QueuesApi(loggerMock.Object);
            app = new AppBase<Settings>();
            app.DataLakeQueue.QueueName = "payloads";
        }

        [TestMethod]
        public async Task GetQueues()
        {
            var res = await function.Get(default);
            var okResult = res as OkObjectResult;
            Assert.IsNotNull(okResult);

            var queues = (okResult.Value as IEnumerable<QueueResponse>)?.Select(o=> o.Body);
            //Assert.IsNotNull(queues);

            var errors = function.App.Log.GetErrorsAndCriticals().ToList();
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public async Task AddDataToDataLakeQueue()
        {
            var a = await app.DataLakeQueue.AddMessageAsync("Hejsaæøå2");

        }

        [TestMethod]
        public async Task PurgeDataToDataLakeQueue()
        {
            await app.DataLakeQueue.DeleteMessagesAsync();
        }
    }
}