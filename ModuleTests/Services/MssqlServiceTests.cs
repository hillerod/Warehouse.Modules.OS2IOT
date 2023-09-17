using Bygdrift.Warehouse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module;
using Module.Services;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleTests.Services
{
    [TestClass]
    public class MssqlServiceTests
    {
        private readonly MssqlService service;
        private readonly AppBase<Settings> app = new();

        public MssqlServiceTests() => service = new MssqlService(app);

        [TestMethod]
        public void GetDataAsync()
        {
            var g = service.GetIotDevices();
            //var h = g.First().OccupancyDevice;
        }


    }
}
