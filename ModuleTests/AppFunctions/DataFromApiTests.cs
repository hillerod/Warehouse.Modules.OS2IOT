using Bygdrift.DataLakeTools;
using Bygdrift.Warehouse.Helpers.Logs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.AppFunctions;
using Moq;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleTests.Refines
{
    [TestClass]
    public class DataFromApiTests
    {
        private readonly Mock<ILogger<DataFromApi>> loggerMock = new();
        private readonly DataFromApi function;

        public DataFromApiTests() => function = new DataFromApi(loggerMock.Object);

        [TestMethod]
        public async Task TimerTrigger()
        {
            await function.App.DataLake.DeleteDirectoryAsync("", FolderStructure.Path);
            function.App.Mssql.DeleteTable("Applications");
            function.App.Mssql.DeleteTable("IotDevices");
            function.App.Mssql.DeleteTable("DeviceModels");

            await function.GetDataFromApi(default);

            var errors = function.App.Log.GetErrorsAndCriticals().ToList();
            Assert.AreEqual(0, errors.Count);

            //Is data uploaded to datalake?:
            Assert.IsTrue(function.App.DataLake.FileExist("Raw", "Data.txt", FolderStructure.DatePath));

            //Is data uploaded to database?:
            var csvFromDb = function.App.Mssql.GetAsCsv("Data");
            //Assert.IsTrue(csvFromDb.RowCount == 2);
        }
    }
}
