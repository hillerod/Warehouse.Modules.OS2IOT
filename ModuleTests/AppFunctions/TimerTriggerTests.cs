using Bygdrift.Tools.CsvTool;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.AppFunctions;
using Moq;
using ScottPlot.Drawing.Colormaps;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleTests.AppFunctions
{
    [TestClass]
    public class TimerTriggerTests
    {
        private readonly Mock<ILogger<TimerTrigger>> loggerMock = new();
        private readonly TimerTrigger function;

        public TimerTriggerTests() => function = new TimerTrigger(loggerMock.Object);

        [TestMethod]
        public async Task LoadFromOS2IOTApi()
        {
            await function.TimerTriggerGetDataFromApi(default);
            var errors = function.App.Log.GetErrorsAndCriticals();
            Assert.IsFalse(errors.Any());
        }

        [TestMethod]
        public async Task TimerTriggerIngestQueuedPayloads()
        {
            var json = "{ \"deveui\":\"a81758fffe043f5d\", \"time\":\"2023-09-07T19:57:30.45625Z\", \"data\":{ \"motion\":2, \"occupancy\":1 } }";
            await function.App.DataLakeQueue.AddMessageAsync(json);
            await function.TimerTriggerIngestQueuedPayloads(default);
        }

        [TestMethod]
        public async Task CalculateOccupancies()
        {
            await function.TimerTriggerCalculateOccupancies(default);
        }
    }
}
