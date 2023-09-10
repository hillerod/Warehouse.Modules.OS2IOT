using Bygdrift.Tools.CsvTool.TimeStacking;
using Bygdrift.Warehouse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module;
using Module.Services;
using Module.Services.OccupanyModels;
using System;
using System.IO;
using System.Linq;

namespace ModuleTests.Services
{
    [TestClass]
    public class OccupancyServiceTests
    {
        public static readonly string BasePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
        private readonly OccupancyService service;
        private readonly AppBase<Settings> app = new();

        public OccupancyServiceTests()
        {
            service = new OccupancyService(app);
        }

        [TestMethod]
        public void GetData()
        {
            var device = GetDevice(false);
            var csvIn = service.GetData(device, 3);
            var timeStack = new TimeStack(csvIn, null, "From", "To").AddInfoFormat("Id", $"{device.DevEUI}-[:From:yyMMddhh]-[:To:yyMMddhh]").AddInfoFrom("From").AddInfoTo("To").AddInfoLength("Occupancy");
            var spans = timeStack.GetSpansPerHour();
            var csv = timeStack.GetTimeStackedCsv(spans);
            //new DrawDigram(2200, 1500, "SampImag.png").DrawTimeStack(spans, false);
            //csvIn.ToCsvFile(Path.Combine(BasePath, "Files", "Out", "Data.csv"));
            //csv.ToCsvFile(Path.Combine(BasePath, "Files", "Out", "spans.csv"));
        }

        private OccupancyDevice GetDevice(bool GetRemote)
        {
            if (GetRemote)
                return service.GetOccupancyDevices().First();

            var json = "{\"OccupancyPerHour\":\"{\\\"deveui\\\": \\\"deveui\\\", \\\"time\\\": \\\"time\\\", \\\"useUTCTime\\\": true, \\\"occupancy\\\": \\\"data.occupancy\\\"}\"}";
            return new OccupancyDevice("a81758fffe043f5d", json);
        }
    }
}
