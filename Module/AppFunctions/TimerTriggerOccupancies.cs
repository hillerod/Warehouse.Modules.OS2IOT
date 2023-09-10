using System.Threading.Tasks;
using Bygdrift.Tools.CsvTool.TimeStacking;
using Bygdrift.Tools.DataLakeTool;
using Bygdrift.Warehouse;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Module.Services;

namespace Module.AppFunctions
{
    public class TimerTriggerOccupancies
    {
        public AppBase<Settings> App { get; }
        public OccupancyService Service { get; }

        public TimerTriggerOccupancies(ILogger<TimerTriggerOccupancies> logger)
        {
            App = new AppBase<Settings>(logger);
            Service = new OccupancyService(App);
        }

        [FunctionName(nameof(TimerTriggerOccupancies))]
        public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            var devices = Service.GetOccupancyDevices();
            foreach (var device in devices)
            {
                var csvIn = Service.GetData(device, 3);
                var timeStack = new TimeStack(csvIn, null, "From", "To").AddInfoFormat("Id", $"{device.DevEUI}-[:From:yyMMddhh]-[:To:yyMMddhh]").AddInfoFormat("DevEUI", device.DevEUI).AddInfoFrom("From").AddInfoTo("To").AddInfoLength("Occupancy");
                var spans = timeStack.GetSpansPerHour();
                var csv = timeStack.GetTimeStackedCsv(spans);
                await App.DataLake.SaveCsvAsync(csv, "OccupancyRefined", "Occupancy.csv", FolderStructure.DateTimePath);
                App.Mssql.MergeCsv(csv, "Occupancies", "Id", false, false);
            }
        }
    }
}
