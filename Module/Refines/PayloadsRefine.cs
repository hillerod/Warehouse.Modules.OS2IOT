using Azure.Storage.Queues.Models;
using Bygdrift.CsvTools;
using Bygdrift.DataLakeTools;
using Bygdrift.Warehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// A refine file, refines incomming data and save it to the public property 'Csv'
/// Afterwards, it describes, how to save the csv to datalake and/or database
/// </summary>
namespace Module.Refines
{
    public class PayloadsRefine
    {
        public static async Task<bool> RefineAsync(AppBase<Settings> app, IEnumerable<QueueMessage> payloads)
        {
            if(payloads == null || !payloads.Any())
                return true;
            
            app.Log.LogInformation("Refining data...");
            var csv = CreateCsv(app, payloads);
            var smallGuid = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", string.Empty).Replace("+", string.Empty)[..10];
            var fileName = $"{app.LoadedLocal:yyyy-MM-dd-HH.mm.ss}_payload_{smallGuid}.csv";
            await app.DataLake.SaveCsvAsync(csv, "PayloadRefined", fileName, FolderStructure.DatePath);
            app.Mssql.InserCsv(csv, "Payloads", false, false);
            return true;
        }
        
        private static Csv CreateCsv(AppBase<Settings> app, IEnumerable<QueueMessage> payloads)
        {
            var now = app.ToLocalTime(DateTime.UtcNow);
            var csv = new Csv();

            foreach (var item in payloads)
            {
                var time = item.InsertedOn.HasValue ? app.ToLocalTime(item.InsertedOn.Value.UtcDateTime) : now;
                var json = Encoding.ASCII.GetString(item.Body);
                csv.FromCsv(new Csv().FromJson(json, true).AddColumn("timeStamp", time, true), false);
            }

            return csv;
        }
    }
}