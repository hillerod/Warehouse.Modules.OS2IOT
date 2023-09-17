using Bygdrift.Tools.CsvTool;
using Bygdrift.Tools.CsvTool.TimeStacking;
using Bygdrift.Tools.DataLakeTool;
using Bygdrift.Warehouse;
using Module.AppFunctions;
using Module.Services.Models.Mssql;
using Module.Services.Models.Occupancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Module.Refines
{
    public class OccupanciesRefine
    {
        private static AppBase<Settings> App;

        public static async Task<Csv> OccupanciesRefineAsync(AppBase<Settings> app, MssqlDevice device, IEnumerable<OccupancyData> data, int delayHours, bool saveToDataLakeAndMsql)
        {
            App = app;
            if (string.IsNullOrEmpty(device.WHOccupancyProperty) && string.IsNullOrEmpty(device.WHOccupancyTableName))
                return null;

            if (string.IsNullOrEmpty(device.WHOccupancyProperty) || string.IsNullOrEmpty(device.WHOccupancyTableName))
            {
                app.Log.LogError($"Device id {device.Id} has only one of two metatags set. Remove or add both: WarehouseOccupancyTableName and WarehouseOccupancyProperty.");
                return null;
            }
            app.Log.LogInformation($"Refining occupancy data for device id {device.Id}...");
            var csv = ConvertFromTo(data);
            Csv res = null;
            var now = DateTime.UtcNow.AddHours(4);
            var from = now.AddHours(-delayHours + 1).Hour;
            var to = now.AddHours(1).Hour;
            if (csv.Records.Any())
            {
                var timeStack = new TimeStack(csv, null, "From", "To").AddInfoFormat("Id", $"{device.DeviceEUI}-[:From:yyMMddHH]").AddInfoFormat("DevEUI", device.DeviceEUI).AddInfoFrom("From").AddInfoTo("To").AddInfoLength("Occupancy");
                var spans = timeStack.GetSpansPerHour(from, to);
                res = timeStack.GetTimeStackedCsv(spans);
                if (saveToDataLakeAndMsql)
                {
                    var fileName = $"{app.LoadedLocal:yyyy-MM-dd-HH.mm.ss}_{device.WHOccupancyTableName}.csv";
                    await app.DataLake.SaveCsvAsync(res, device.WHOccupancyTableName + "Refined", fileName, FolderStructure.DatePath);
                    app.Mssql.MergeCsv(res, device.WHOccupancyTableName, "Id", false, false);
                }
            }
            return res;
        }

        private static Csv ConvertFromTo(IEnumerable<OccupancyData> data)
        {
            var csv = new Csv(App.CsvConfig, "From, To");
            DateTime? from = null;
            foreach (var item in data)
            {
                if (from == null && item.Occupancy >= 1)
                    from = item.Time;
                if (item.Occupancy == 0 && from != null)
                {
                    csv.AddRow(from, item.Time);
                    from = null;
                }
            }
            return csv;
        }
    }
}