using Bygdrift.Warehouse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module;
using Module.Refines;
using System;
using System.Threading.Tasks;
using System.Web;

namespace ModuleTests.Refines
{
    [TestClass]
    public class PayloadRefineTests
    {
        private readonly AppBase<Settings> app = new();
        private readonly string json = "{\"temperature\":23.9,\"humidity\":39,\"light\":234,\"motion\":0,\"co2\":412,\"vdd\":3645,\"deviceId\":\"a81758fffe03d633\",\"location\":{\"type\":\"Point\",\"coordinates\":[12.260736823,55.640562442]},\"commentOnLocation\":\"midt i rodebunken\",\"name\":\"el123\"}";

        [TestMethod]
        public async Task ImportJson()
        {
            var res = "";
            for (int i = 0; i < 140; i++)
            {
                res += Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", string.Empty).Replace("+", string.Empty)[..10] + "\n";

            }
            //var smallGuid = HttpUtility.UrlEncode(a);

            //await PayloadRefine.Refine(app, json);
        }
    }
}
