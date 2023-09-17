using Azure.Storage.Queues.Models;
using Bygdrift.Tools.CsvTool;
using Bygdrift.Tools.DataLakeTool;
using Bygdrift.Warehouse;
using Module.Services.Models.Mssql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A refine file, refines incomming data and save it to the public property 'Csv'
/// Afterwards, it describes, how to save the csv to datalake and/or database
/// </summary>
namespace Module.Refines
{
    public class QueuesRefine
    {
        public static async Task<Dictionary<string, List<JToken>>> RefineAsync(AppBase<Settings> app, IEnumerable<QueueMessage> payloads, MssqlDevices iotDevices, bool saveToDataLakeAndMsql)
        {
            if (payloads == null || !payloads.Any())
                return null;

            app.Log.LogInformation("Refining data...");
            var res = CreateCsv(payloads, iotDevices);
            if (res.Any())
            {
                if (saveToDataLakeAndMsql)
                {
                    var json = JsonConvert.SerializeObject(res);
                    var smallGuid = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", string.Empty).Replace("+", string.Empty)[..10];
                    var fileName = $"{app.LoadedLocal:yyyy-MM-dd-HH.mm.ss}_payload_{smallGuid}.csv";
                    await app.DataLake.SaveObjectAsync(json, "PayloadRefined", fileName, FolderStructure.DatePath);

                    foreach (var item in res)
                    {
                        var csv = new Csv();
                        foreach (var jToken in item.Value)
                            csv.AddJson(jToken, false);

                        app.Mssql.InsertCsv(csv, item.Key, false, false);
                    }
                }
            }
            return res;
        }

        public static JArray RefineToJson(AppBase<Settings> app, IEnumerable<QueueMessage> payloads, string groupHeader = null)
        {
            if (payloads == null || !payloads.Any())
                return null;

            var csv = CreateCsv(payloads);
            if (string.IsNullOrEmpty(groupHeader))
                return csv.ToJArray();

            var groupHeaderNo = csv.GetHeader(groupHeader);
            var groupedRecords = csv.GetColRecords<string>(groupHeader);
            var res = new JArray();
            foreach (var item in groupedRecords.GroupBy(o => o.Value))
            {
                var arr = new JArray();
                foreach (int row in item.Select(o => o.Key))
                {
                    var dataGroup = new JObject();
                    foreach (var record in csv.GetRowRecords(row).Where(o => o.Key != groupHeaderNo))
                        dataGroup.Add(csv.Headers[record.Key], record.Value != null ? JToken.FromObject(record.Value) : null);

                    arr.Add(dataGroup);
                }
                res.Add(new JObject { { groupHeader, item.Key }, { "group", arr } });
            }
            return res; //.ToString(Newtonsoft.Json.Formatting.None);
        }

        private static Csv CreateCsv(IEnumerable<QueueMessage> payloads)
        {
            var res = new Csv();
            foreach (var item in payloads)
            {
                var json = Encoding.ASCII.GetString(item.Body);
                var csvIn = new Csv().AddJson(json, true);
                res.AddCsv(csvIn);
            }
            return res;
        }

        /// <summary>A dictionary where key is warehouseTableName and value is a list of payloads</summary>
        private static Dictionary<string, List<JToken>> CreateCsv(IEnumerable<QueueMessage> payloads, MssqlDevices iotDevices)
        {
            var res = new Dictionary<string, List<JToken>>();
            foreach (var item in payloads)
            {
                var json = Encoding.ASCII.GetString(item.Body);
                var jToken = JToken.Parse(json);
                var warehouseTableName = GetWarehouseTable(jToken, iotDevices);
                if (warehouseTableName != null)
                {
                    if (res.ContainsKey(warehouseTableName))
                        res[warehouseTableName].Add(jToken);
                    else
                        res.Add(warehouseTableName, new List<JToken>() { jToken });
                }
            }
            return res;
        }

        private static string GetWarehouseTable(JToken jToken, MssqlDevices iotDevices)
        {
            foreach (var item in iotDevices.DevEUINames)
            {
                var deveui = jToken[item]?.Value<string>();
                if (deveui != null)
                {
                    var res = iotDevices.IOTDevices.SingleOrDefault(o => o.DeviceEUI == deveui);
                    if (res != null)
                        return res.WHTableName;
                }
            }
            return null;
        }
    }
}