using Bygdrift.Tools.DataLakeTool;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.AppFunctions;
using Module.Refines;
using Module.Services;
using Module.Services.OS2IOTModels;
using Moq;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleTests.AppFunctions.OS2IOT
{
    [TestClass]
    public class QueuesTests
    {
        private readonly Mock<ILogger<TimerTriggerGetDataFromApi>> loggerMock = new();
        private readonly TimerTriggerGetDataFromApi function;

        public QueuesTests() => function = new TimerTriggerGetDataFromApi(loggerMock.Object);

        [TestMethod]
        public async Task GetData()
        {
            var ApiService = new OS2IOTApiService(function.App);
            var applications = await ApiService.GetApplicationsAsync();
            var iotDevices = await ApiService.GetIOTDevicesAsync(applications);
            var models = await ApiService.GetDeviceModelsAsync();

            //Only possible if administrator:
            ///var organizations = await ApiService.GetOrganizationsAsync();
            /// var chirpstackGateways = await ApiService.GetChirpstackGatewaysAsync(organizations);

            await ApiRefine.RefineAsync(function.App, false, null, applications, models, iotDevices, null);
        }

        [TestMethod]
        public async Task TimerTrigger()
        {
            await function.App.DataLake.DeleteDirectoryAsync("", FolderStructure.Path);
            function.App.Mssql.DeleteTable("Organizations");
            function.App.Mssql.DeleteTable("ChirpstackGateways");
            function.App.Mssql.DeleteTable("Applications");
            function.App.Mssql.DeleteTable("DeviceModels");
            function.App.Mssql.DeleteTable("IotDevices");

            await function.Run(default);

            var errors = function.App.Log.GetErrorsAndCriticals().ToList();
            Assert.AreEqual(0, errors.Count);

            ////Is data uploaded to datalake?:
            //Assert.IsTrue(function.App.DataLake.FileExist("Raw", "Data.txt", FolderStructure.DatePath));

            ////Is data uploaded to database?:
            //var csvFromDb = function.App.Mssql.GetAsCsv("Data");
            ////Assert.IsTrue(csvFromDb.RowCount == 2);
        }
    }
}
