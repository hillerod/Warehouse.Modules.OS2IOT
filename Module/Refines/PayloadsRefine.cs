using Azure.Storage.Queues.Models;
using Bygdrift.Tools.CsvTool;
using Bygdrift.Tools.DataLakeTool;
using Bygdrift.Warehouse;
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
    public class PayloadsRefine
    {
        public static async Task<Csv> RefineAsync(AppBase<Settings> app, IEnumerable<QueueMessage> payloads, bool saveToDataLakeAndMsql)
        {
            if (payloads == null || !payloads.Any())
                return null;

            app.Log.LogInformation("Refining data...");
            var csv = CreateCsv(app, payloads);
            var smallGuid = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", string.Empty).Replace("+", string.Empty)[..10];
            var fileName = $"{app.LoadedLocal:yyyy-MM-dd-HH.mm.ss}_payload_{smallGuid}.csv";
            if (saveToDataLakeAndMsql)
            {
                await app.DataLake.SaveCsvAsync(csv, "PayloadRefined", fileName, FolderStructure.DatePath);
                app.Mssql.InsertCsv(csv, "Payloads", false, false);
            }
            return csv;
        }

        private static Csv CreateCsv(AppBase<Settings> app, IEnumerable<QueueMessage> payloads)
        {
            var now = app.ToLocalTime(DateTime.UtcNow);
            var res = new Csv();

            foreach (var item in payloads)
            {
                var time = item.InsertedOn.HasValue ? app.ToLocalTime(item.InsertedOn.Value.UtcDateTime) : now;
                var json = Encoding.ASCII.GetString(item.Body);
                res.AddCsv(new Csv().AddJson(json, true).AddColumn("QueueReceived", time, true));
            }

            return res;
        }
    }
}