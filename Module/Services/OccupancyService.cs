using Bygdrift.Tools.CsvTool;
using Bygdrift.Warehouse;
using Module.Services.OccupanyModels;
using RepoDb;
using System;
using System.Collections.Generic;

namespace Module.Services
{
    public class OccupancyService
    {
        public AppBase<Settings> App { get; }

        public OccupancyService(AppBase<Settings> app) => App = app;

        public IEnumerable<OccupancyDevice> GetOccupancyDevices()
        {
            var sql = "SELECT deviceEUI, metadata \n" +
                $"FROM [{App.ModuleName}].IotDevices \n" +
                "WHERE metadata LIKE '%\"OccupancyPerHour\"%'";

            foreach (dynamic item in App.Mssql.Connection.ExecuteQuery(sql))
                yield return new OccupancyDevice(item.deviceEUI, item.metadata);
        }

        public Csv GetData(OccupancyDevice device, int delayHours)
        {
            var time = (device.UseUTCTime ? DateTime.UtcNow.AddHours(-delayHours+1) : DateTime.Now.AddHours(-delayHours+1));
            time = time.Date.AddHours(time.Hour);
            var sql = $"SELECT [{device.TimeColumn}] as [Time], [{device.OccupancyColumn}] as [Occupancy]\n" +
                $"FROM [{App.ModuleName}].Payloads \n" +
                $"WHERE [{device.DevEUIColumn}] = '{device.DevEUI}' AND [{device.TimeColumn}] >= '{time:yyyy-MM-dd HH:mm:ss}'\n" +
                $"ORDER BY [{device.TimeColumn}]";

            var data = App.Mssql.Connection.ExecuteQuery<OccupancyTime>(sql);
            var csv = new Csv("From, To");
            DateTime? from = null;
            foreach (var item in data)
            {
                if (from == null && item.Occupancy >= 1)
                    from = item.Time;
                if(item.Occupancy == 0 && from != null)
                {
                    csv.AddRow(from, item.Time);
                    from = null;
                }
            }
            return csv;
        }
    }
}
