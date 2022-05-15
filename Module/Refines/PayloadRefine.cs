using Bygdrift.CsvTools;
using Bygdrift.DataLakeTools;
using Bygdrift.Warehouse;
using System;
using System.Threading.Tasks;

/// <summary>
/// A refine file, refines incomming data and save it to the public property 'Csv'
/// Afterwards, it describes, how to save the csv to datalake and/or database
/// </summary>
namespace Module.Refines
{
    public class PayloadRefine
    {
        public static async Task Refine(AppBase<Settings> app, string json)
        {
            app.Log.LogInformation("Refining data...");
            var smallGuid = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("==", string.Empty);
            var fileName = $"{DateTime.Now:yyyy-MM-dd HH.mm.ss}_{smallGuid}_payload.json";
            await app.DataLake.SaveStringAsync(json, "PayloadRaw", fileName, FolderStructure.DateTimePath);
            var csv = CreateCsv(app, json);
            app.Mssql.InserCsv(csv, "Payloads", false, false);
        }

        private static Csv CreateCsv(AppBase<Settings> app, string json)
        {
            var csv = new Csv().FromJson(json);
            csv.AddRecord(1, "timeStamp", app.ToLocalTime(DateTime.Now));
            return csv;
        }
    }
}