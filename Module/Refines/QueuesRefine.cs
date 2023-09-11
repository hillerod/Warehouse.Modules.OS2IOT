using Azure.Storage.Queues.Models;
using Bygdrift.Tools.CsvTool;
using Bygdrift.Tools.DataLakeTool;
using Bygdrift.Warehouse;
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
        public static async Task<Csv> RefineAsync(AppBase<Settings> app, IEnumerable<QueueMessage> payloads, bool saveToDataLakeAndMsql)
        {
            if (payloads == null || !payloads.Any())
                return null;

            app.Log.LogInformation("Refining data...");
            var csv = CreateCsv(app, payloads, true);
            var smallGuid = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", string.Empty).Replace("+", string.Empty)[..10];
            var fileName = $"{app.LoadedLocal:yyyy-MM-dd-HH.mm.ss}_payload_{smallGuid}.csv";
            if (saveToDataLakeAndMsql)
            {
                await app.DataLake.SaveCsvAsync(csv, "PayloadRefined", fileName, FolderStructure.DatePath);
                app.Mssql.InsertCsv(csv, "Payloads", false, false);
            }
            return csv;
        }

        public static JArray RefineToJson(AppBase<Settings> app, IEnumerable<QueueMessage> payloads, string groupHeader = null)
        {
            if (payloads == null || !payloads.Any())
                return null;

            var csv = CreateCsv(app, payloads, false);
            if(string.IsNullOrEmpty(groupHeader))
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
                        dataGroup.Add(csv.Headers[record.Key], record.Value != null ? JToken.FromObject(record.Value) : null );

                    arr.Add(dataGroup);
                }
                res.Add(new JObject { { groupHeader, item.Key }, { "group", arr } });
            }
            return res; //.ToString(Newtonsoft.Json.Formatting.None);
        }

        private static Csv CreateCsv(AppBase<Settings> app, IEnumerable<QueueMessage> payloads, bool addQueuesRecieved)
        {
            var now = app.ToLocalTime(DateTime.UtcNow);
            var res = new Csv();

            foreach (var item in payloads)
            {
                var time = item.InsertedOn.HasValue ? app.ToLocalTime(item.InsertedOn.Value.UtcDateTime) : now;
                var json = Encoding.ASCII.GetString(item.Body);
                var csvIn = new Csv().AddJson(json, true);
                if (addQueuesRecieved)
                    csvIn.AddColumn("QueueReceived", time, true);

                res.AddCsv(csvIn);
            }

            return res;
        }
    }
}