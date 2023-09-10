using Bygdrift.Warehouse;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.AppFunctions;
using Moq;
using System.Threading.Tasks;

namespace ModuleTests.AppFunctions
{
    [TestClass]
    public class TimerTriggerOccupanciesTests
    {
        private readonly Mock<ILogger<TimerTriggerOccupancies>> loggerMock = new();
        private readonly TimerTriggerOccupancies function;
        private readonly AppBase<Settings> app;

        public TimerTriggerOccupanciesTests()
        {
            function = new TimerTriggerOccupancies(loggerMock.Object);
            app = new AppBase<Settings>();
        }

        [TestMethod]
        public async Task GetAndDelete()
        {
            await function.Run(default, default);
        }
    }
}
