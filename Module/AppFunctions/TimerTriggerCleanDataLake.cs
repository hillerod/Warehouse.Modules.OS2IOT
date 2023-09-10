using Bygdrift.Warehouse;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Module.AppFunctions
{
    public class TimerTriggerCleanDataLake
    {
        public AppBase<Settings> App { get; }

        public TimerTriggerCleanDataLake(ILogger<TimerTriggerGetDataFromApi> logger)
        {
            App = new AppBase<Settings>(logger);
            //App.Log.LogInformation($"CTOR PATH datalekContainer: {App.DataLake.Container}");
            //App.Log.LogInformation($"CTOR PATH: {App.DataLakeQueue.ConnectionString}, container: {App.DataLakeQueue.Container}, name: {App.DataLakeQueue.Name}");
        }

        [FunctionName(nameof(TimerTriggerCleanDataLake))]
        public async Task Run([TimerTrigger("0 0 0 * * 0")] TimerInfo myTimer)
        {
            var days = Convert.ToInt32((DateTime.Now - DateTime.Now.AddMonths(-App.Settings.MonthsToKeepDataInDataLake)).TotalDays);
            await App.DataLake.DeleteDirectoriesOlderThanDaysAsync("ApiRaw", days);
            await App.DataLake.DeleteDirectoriesOlderThanDaysAsync("ApiRefined", days);

            App.Log.LogInformation($"PATH datalekContainer: {App.DataLake.Container}");
            App.Log.LogInformation($"PATH: {App.DataLakeQueue.ConnectionString}, container: {App.DataLakeQueue.Container}, name: {App.DataLakeQueue.Name}");
        }
    }
}