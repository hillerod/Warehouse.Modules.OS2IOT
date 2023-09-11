using System.Linq;
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
            if (!App.Settings.CalculateOccupancyPerHour)
                return;

            var devices = Service.GetOccupancyDevices();
            foreach (var device in devices)
            {
                var csvIn = Service.GetData(device, 15);
                if (csvIn.Records.Any())
                {
                    var timeStack = new TimeStack(csvIn, null, "From", "To").AddInfoFormat("Id", $"{device.DevEUI}-[:From:yyMMddHH]-[:To:yyMMddHH]").AddInfoFormat("DevEUI", device.DevEUI).AddInfoFrom("From").AddInfoTo("To").AddInfoLength("Occupancy");
                    var spans = timeStack.GetSpansPerHour();
                    var csv = timeStack.GetTimeStackedCsv(spans);
                    var fileName = $"{App.LoadedLocal:yyyy-MM-dd-HH.mm.ss}_occupancy.csv";
                    await App.DataLake.SaveCsvAsync(csv, "OccupancyRefined", fileName, FolderStructure.DatePath);
                    App.Mssql.MergeCsv(csv, "Occupancies", "Id", false, false);
                }
            }
        }
    }
}
