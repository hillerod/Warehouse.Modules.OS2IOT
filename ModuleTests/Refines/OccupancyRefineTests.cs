using Bygdrift.Warehouse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module;
using Module.Refines;
using System.Threading.Tasks;

namespace ModuleTests.Refines
{
    [TestClass]
    public class OccupancyRefineTests
    {
        private readonly AppBase<Settings> app = new();

        [TestMethod]
        public async Task Import()
        {
            var delay = 3;
            var iotDevices = await Helper.GetMssqlIotDevices(app);
            foreach (var device in iotDevices.IOTDevices)
            {
                //Fra 2023-09-14 09:25:49.000 til 2023-09-14 16:37:34.000
                var data = Helper.GetPayloadData(app, device, delay);
                var csv = await OccupanciesRefine.OccupanciesRefineAsync(app, device, data, delay, false);
                csv.ToCsvFile("c:\\Users\\kenbo\\Downloads\\payloadsOut.csv");
            }
        }
    }
}
