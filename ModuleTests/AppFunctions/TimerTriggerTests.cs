using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.AppFunctions;
using Moq;
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
        public async Task CalculateOccupancies()
        {
            await function.TimerTriggerCalculateOccupancies(default);
        }
    }
}
