using Azure.Storage.Queues.Models;
using Bygdrift.Warehouse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module;
using Module.Refines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModuleTests.Refines
{
    [TestClass]
    public class QueuesRefineTests
    {
        private readonly AppBase<Settings> app = new();
        //private readonly string json = "{\"temperature\":23.9,\"humidity\":39,\"light\":234,\"motion\":0,\"co2\":412,\"vdd\":3645,\"deviceId\":\"a81758fffe03d633\",\"location\":{\"type\":\"Point\",\"coordinates\":[12.260736823,55.640562442]},\"commentOnLocation\":\"midt i rodebunken\",\"name\":\"el123\"}";
        private readonly string json = "{ \"deveui\":\"a81758fffe043f5d\", \"time\":\"2023-09-07T19:57:30.45625Z\", \"data\":{ \"motion\":2, \"occupancy\":1 } }";
        private readonly string json2 = "{ \"deveui\":\"a81758fffe043f5d\", \"time\":\"2023-09-07T19:57:30.45625Z\", \"data\":{} }";

        [TestMethod]
        public async Task ImportJson()
        {
            var queues = new List<QueueMessage>()
            {
                QueuesModelFactory.QueueMessage("id", "pr", json, 1, null, DateTimeOffset.UtcNow),
                QueuesModelFactory.QueueMessage("id", "pr", json2, 1, null, DateTimeOffset.UtcNow),
            };

            var iotDevices = await Helper.GetMssqlIotDevices(app);
            var res = await QueuesRefine.RefineAsync(app, queues, iotDevices, false);
            //Assert.AreEqual(6, res.Records.Count);
        }

        [TestMethod]
        public async Task ImportJson2()
        {
            var queues = new List<QueueMessage>()
            {
                QueuesModelFactory.QueueMessage("id", "pr", "{ \"deveui\":\"a81758fffe043f5d\", \"time\":\"2023-09-07T19:57:30.45625Z\", \"data\":{ \"motion\":2, \"occupancy\":1 } }", 1, null, DateTimeOffset.UtcNow),
                QueuesModelFactory.QueueMessage("id", "pr", "{ \"deveui\":\"a81758fffe043f5d\", \"time\":\"2023-09-07T19:57:30.45625Z\", \"data\":{ \"motion\":0, \"occupancy\":1 } }", 1, null, DateTimeOffset.UtcNow),
                QueuesModelFactory.QueueMessage("id", "pr", "{ \"deveui\":\"a81758fffe043f5a\", \"time\":\"2023-09-07T19:57:30.45625Z\", \"data\":{ \"motion\":0, \"occupancy\":1 } }", 1, null, DateTimeOffset.UtcNow),
            };

            var json = QueuesRefine.RefineToJson(app, queues, "deveui");
            Assert.AreEqual("[{\"deveui\":\"a81758fffe043f5d\",\"group\":[{\"time\":\"2023-09-07T19:57:30.45625Z\",\"data.motion\":2,\"data.occupancy\":1},{\"time\":\"2023-09-07T19:57:30.45625Z\",\"data.motion\":0,\"data.occupancy\":1}]},{\"deveui\":\"a81758fffe043f5a\",\"group\":[{\"time\":\"2023-09-07T19:57:30.45625Z\",\"data.motion\":0,\"data.occupancy\":1}]}]", json);

            json = QueuesRefine.RefineToJson(app, queues);
            Assert.AreEqual("[{\"deveui\":\"a81758fffe043f5d\",\"time\":\"2023-09-07T19:57:30.45625Z\",\"data.motion\":2,\"data.occupancy\":1},{\"deveui\":\"a81758fffe043f5d\",\"time\":\"2023-09-07T19:57:30.45625Z\",\"data.motion\":0,\"data.occupancy\":1},{\"deveui\":\"a81758fffe043f5a\",\"time\":\"2023-09-07T19:57:30.45625Z\",\"data.motion\":0,\"data.occupancy\":1}]", json);
        }

        /// <summary>Sends a lot of parallel messages to an address. Not intended to be run automatic</summary>
        [TestMethod]
        public async Task StressSystem()
        {
            var repeats = 10;
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);
            client.BaseAddress = new Uri(app.HostName);
            client.Timeout = new TimeSpan(1, 0, 0);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + app.Settings.OS2IOTAuthorization);
            var tasks = new List<Task<HttpResponseMessage>>();
            var start = DateTime.Now;

            for (int i = 1; i <= repeats; i++)
            {
                //var json = "{\"batch\":\"" + app.LoadedLocal + "\",\"batchId\":" + i + "}";
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                //await client.PostAsync("/api/Payload", data);

                tasks.Add(client.PostAsync("/api/Payload", data));
            }

            await Task.WhenAll(tasks);
            var stopped = (DateTime.Now - start).TotalSeconds;

            var errors = tasks.Select(o => o.Result.StatusCode == HttpStatusCode.OK).Count(o => o == false);
            Assert.AreEqual(0, errors);
        }
    }
}
