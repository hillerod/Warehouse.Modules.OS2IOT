using Bygdrift.Warehouse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module;
using Module.Refines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ModuleTests.Refines
{
    [TestClass]
    public class PayloadRefineTests
    {
        private readonly AppBase<Settings> app = new();
        //private readonly string json = "{\"temperature\":23.9,\"humidity\":39,\"light\":234,\"motion\":0,\"co2\":412,\"vdd\":3645,\"deviceId\":\"a81758fffe03d633\",\"location\":{\"type\":\"Point\",\"coordinates\":[12.260736823,55.640562442]},\"commentOnLocation\":\"midt i rodebunken\",\"name\":\"el123\"}";

        //[TestMethod]
        //public async Task ImportJson()
        //{
        //    //await PayloadsRefine.Refine(app, json);
        //}

        /// <summary>Sends a lot of parallel messages to an address. Not intended to be run automatic</summary>
        [TestMethod]
        public async Task StressSystem()
        {
            var repeats = 100;
            var start = DateTime.Now;
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);
            client.BaseAddress = new Uri("http://localhost:7071");
            client.Timeout = new TimeSpan(1, 0, 0);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + app.Settings.PostPayloadsAuthorizationKey);
            var tasks = new List<Task<HttpResponseMessage>>();

            for (int i = 1; i <= repeats; i++)
            {
                var json = "{\"batch\":\"" + app.LoadedLocal + "\",\"batchId\":" + i + "}";
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                tasks.Add(client.PostAsync("/api/Payload", data));
            }

            await Task.WhenAll(tasks);
            var stopped = (DateTime.Now - start).TotalSeconds;

            var errors = tasks.Select(o => o.Result.StatusCode == HttpStatusCode.OK).Count(o=> o == false);
            Assert.AreEqual(0, errors);
        }
    }
}
