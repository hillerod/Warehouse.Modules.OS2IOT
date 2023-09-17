using Bygdrift.Warehouse;
using Module;
using Module.Services;
using Module.Services.Models.Mssql;
using Module.Services.Models.Occupancy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleTests
{
    public class Helper
    {
        public static readonly string BasePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

        public static async Task<MssqlDevices> GetMssqlIotDevices(AppBase<Settings> app)
        {
            var path = Path.Combine(BasePath, "Files", "In", "IOTDevices.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<MssqlDevices>(json);
            }

            var os2IOTService = new OS2IOTApiService(app);
            var applications = await os2IOTService.GetApplicationsAsync();
            var data = await os2IOTService.GetIOTDevicesAsync(applications);
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
            return data;
        }

        public static IEnumerable<OccupancyData> GetPayloadData(AppBase<Settings> app, MssqlDevice device, int delayHours)
        {
            var path = Path.Combine(BasePath, "Files", "In", $"IOTDevice {device.Id}_Data.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<IEnumerable<OccupancyData>>(json);
            }

            var occupancyService = new OccupancyService(app);
            var data = occupancyService.GetData(device, delayHours);
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
            return data;
        }
    }
}
