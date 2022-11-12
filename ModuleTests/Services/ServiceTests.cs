using Bygdrift.Warehouse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module;
using Module.Services;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleTests.Services
{
    [TestClass]
    public class ServiceTests
    {
        private readonly OS2IOTApiService service;
        private readonly AppBase<Settings> app = new();

        public ServiceTests()
        {
            service = new OS2IOTApiService(app);
        }

        [TestMethod]
        public async Task GetDataAsync()
        {
            var g = await service.GetAuthProfileAsync();
        }

        [TestMethod]
        public async Task GetGatewaysAsync()
        {
            var g = await service.GetChirpstackGatewaysAsync(10);
            var h = g.ToList();
        }
    }
}
