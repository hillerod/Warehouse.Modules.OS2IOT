using Bygdrift.Warehouse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module;
using Module.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleTests.Services
{
    [TestClass]
    public class IServiceTests
    {
        private readonly ApiService service;
        private readonly AppBase<Settings> app = new();

        public IServiceTests()
        {
            service = new ApiService(app);
        }

        [TestMethod]
        public async Task GetDataAsync()
        {
            var g = await service.GetAuthProfileAsync();
        }
    }
}
