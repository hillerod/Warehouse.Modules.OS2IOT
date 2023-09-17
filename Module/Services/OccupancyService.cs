using Bygdrift.Tools.CsvTool;
using Bygdrift.Warehouse;
using Module.Services.Models.Mssql;
using Module.Services.Models.Occupancy;
using RepoDb;
using System;
using System.Collections.Generic;

namespace Module.Services
{
    public class OccupancyService
    {
        public AppBase<Settings> App { get; }

        public OccupancyService(AppBase<Settings> app) => App = app;

        public IEnumerable<OccupancyData> GetData(MssqlDevice device, int delayHours)
        {
            if (!string.IsNullOrEmpty(device.WHOccupancyProperty) && !string.IsNullOrEmpty(device.WHTableName) && !string.IsNullOrEmpty(device.WHTimeProperty) && !string.IsNullOrEmpty(device.WHDevEuiProperty))
            {
                var time = DateTime.UtcNow.AddHours(-delayHours + 1);
                time = time.Date.AddHours(time.Hour);
                var sql = $"SELECT [{device.WHTimeProperty}] as [Time], [{device.WHOccupancyProperty}] as [Occupancy]\n" +
                    $"FROM [{App.ModuleName}].[{device.WHTableName}] \n" +
                    $"WHERE [{device.WHDevEuiProperty}] = '{device.DeviceEUI}' AND [{device.WHTimeProperty}] >= '{time:yyyy-MM-dd HH:mm:ss}'\n" +
                    $"ORDER BY [{device.WHTimeProperty}]";
                
                return App.Mssql.Connection.ExecuteQuery<OccupancyData>(sql);
            }
            return null;
        }
    }
}
