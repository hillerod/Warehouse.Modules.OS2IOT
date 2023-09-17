using Bygdrift.Tools.CsvTool;
using Bygdrift.Tools.DataLakeTool;
using Bygdrift.Warehouse;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Module.Refines;
using Module.Services;
using Module.Services.Models.Mssql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Module.AppFunctions
{
    /// <summary>
    /// Visits OS2IOT and gathers all relevant data and saves it to the warehouse
    /// </summary>
    public class TimerTrigger
    {
        ///VÆR SIKKER PÅ AT DENNE IKKE køres hvert ti minut!!!
        private MssqlDevices _iotDevices;

        public AppBase<Settings> App { get; }

        public MssqlDevices IOTDevices
        {
            get
            {
                if(_iotDevices == null)
                {
                    _iotDevices =new MssqlDevices(new MssqlService(App).GetIotDevices());
                    var fileName = $"{App.LoadedLocal:yyyy-MM-dd-HH.mm.ss}_IOTCalls.txt";
                    Task.Run(async () => { await App.DataLake.SaveStringAsync("IotDevices are called at " + DateTime.Now.ToString(), "Calls", fileName, FolderStructure.DatePath); });
                }
                return _iotDevices;
            }
            set { _iotDevices = value; }
        }

        public TimerTrigger(ILogger<TimerTrigger> logger)
        {
            App = new AppBase<Settings>(logger);
            App.DataLakeQueue.QueueName = "payloads";
            App.CsvConfig.DateFormatKind = FormatKind.TimeOffsetDST;
        }

        [FunctionName("TimerTriggerIngestQueuedPayloads")]
        public async Task TimerTriggerIngestQueuedPayloads([TimerTrigger("0 */10 * * * *")] TimerInfo myTimer)
        {
            if (!App.Settings.IngestQueuedPayloads)
                return;

            App.Log.LogInformation("Ingesting queue payloads");
            var messages = await App.DataLakeQueue.GetMessagesAsync();
            if (await QueuesRefine.RefineAsync(App, messages, IOTDevices, true) != null)
                await App.DataLakeQueue.DeleteMessagesAsync(messages);
        }

        [FunctionName("TimerTriggerGetDataFromApi")]
        public async Task TimerTriggerGetDataFromApi([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
        {
            App.Log.LogInformation("Loading data from OS2IOT");
            var apiService = new OS2IOTApiService(App);
            var organizations = App.Settings.GetOS2IOTApiOrganizationAndGateways ? await apiService.GetOrganizationsAsync() : null;
            var applications = await apiService.GetApplicationsAsync();
            if (applications == null)
                return;

            var models = await apiService.GetDeviceModelsAsync();
            IOTDevices = await apiService.GetIOTDevicesAsync(applications);
            var chirpstackGateways = App.Settings.GetOS2IOTApiOrganizationAndGateways ? await apiService.GetChirpstackGatewaysAsync(organizations) : null;
            await ApiRefine.RefineAsync(App, true, organizations, applications, models, IOTDevices, chirpstackGateways);
        }

        [FunctionName("TimerTriggerCalculateOccupancies")]
        public async Task TimerTriggerCalculateOccupancies([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
        {
            if (!App.Settings.CalculateOccupancyPerHour)
                return;

            App.Log.LogInformation("Calculating occupancies");
            var delayHours = 15;
            var service = new OccupancyService(App);
            foreach (var device in IOTDevices.IOTDevices)
                await OccupanciesRefine.OccupanciesRefineAsync(App, device, service.GetData(device, delayHours), delayHours, true);
        }

        [FunctionName("TimerTriggerCleanDataLake")]
        public async Task TimerTriggerCleanDataLake([TimerTrigger("0 0 0 * * 0")] TimerInfo myTimer)
        {
            var days = Convert.ToInt32((DateTime.Now - DateTime.Now.AddMonths(-App.Settings.MonthsToKeepDataInDataLake)).TotalDays);
            await App.DataLake.DeleteDirectoriesOlderThanDaysAsync("ApiRaw", days);
            await App.DataLake.DeleteDirectoriesOlderThanDaysAsync("ApiRefined", days);
        }
    }
}

