//using Bygdrift.DataLakeTools;
//using Bygdrift.Warehouse.Helpers.Logs;
//using Microsoft.Extensions.Logging;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Module.AppFunctions;
//using Moq;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ModuleTests.Refines
//{
//    /// To run this test, then first add an Azure environmed, as decribed here: https://github.com/Bygdrift/Warehouse
//    /// Then fetch the Azure App config connections tring and paste it to this project's User Secret like: {"ConnectionStrings:AppConfig": "the connectionstring to app config"}.
//    [TestClass]
//    public class TimerTriggerTests
//    { 
//        private readonly Mock<ILogger<TimerTrigger>> loggerMock = new();
//        private readonly TimerTrigger function;

//        public TimerTriggerTests()
//        {
//            function = new TimerTrigger(loggerMock.Object);
//        }

//        [TestMethod]
//        public async Task TimerTrigger()
//        {
//            //Clear the data in the warehouse for this module:
//            await function.App.DataLake.DeleteDirectoryAsync("", FolderStructure.Path);

//            //Clear the database for the two tables:
//            function.App.Mssql.DeleteTable("Data");
//            function.App.Mssql.DeleteTable("DataAccumulated");

//            //Run the function:
//            await function.TimerTriggerAsync(default);

//            //There should come no errors
//            var errors = function.App.Log.GetErrorsAndCriticals().ToList();
//            Assert.AreEqual(0, errors.Count);

//            //There should come  a warning that 'DataNoteSet' is not set
//            var warnings = function.App.Log.GetLogs(LogType.Warning).ToList();
//            Assert.AreEqual(1, warnings.Count);

//            //There should come  2 informations, that th module is started and that it is refining data
//            var infos = function.App.Log.GetLogs(LogType.Information).ToList();
//            Assert.AreEqual(2, infos.Count);

//            //Is data uploaded to datalake?:
//            Assert.IsTrue(function.App.DataLake.FileExist("Raw", "Data.txt", FolderStructure.DatePath));
//            Assert.IsTrue(function.App.DataLake.FileExist("Refined", "Data.csv", FolderStructure.DatePath));

//            //Is data uploaded to database?:
//            var csvFromDb = function.App.Mssql.GetAsCsv("Data");
//            Assert.IsTrue(csvFromDb.RowCount == 2);

//            //Is data uploaded to database accumulated?:
//            var csvFromDbAccumulated = function.App.Mssql.GetAsCsv("DataAccumulated");
//            Assert.IsTrue(csvFromDbAccumulated.RowCount >= 2);
//        }
//    }
//}
