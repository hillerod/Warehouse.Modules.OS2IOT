using Bygdrift.Warehouse;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Module.AppFunctions.OS2IOT
{
    public class Os2IOT_CleanDataLake
    {
        public Os2IOT_CleanDataLake(ILogger<Os2IOT_GetDataFromApi> logger) => App = new AppBase<Settings>(logger);

        public AppBase<Settings> App { get; }

        [FunctionName(nameof(Os2IOT_CleanDataLake))]
        public async Task Run([TimerTrigger("0 0 0 * * 0")] TimerInfo myTimer)
        {
            var days = Convert.ToInt32((DateTime.Now - DateTime.Now.AddMonths(-App.Settings.MonthsToKeepDataInDataLake)).TotalDays);
            await App.DataLake.DeleteDirectoriesOlderThanDaysAsync("ApiRaw", days);
            await App.DataLake.DeleteDirectoriesOlderThanDaysAsync("ApiRefined", days);
        }
    }
}