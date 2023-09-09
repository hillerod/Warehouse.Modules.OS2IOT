using Bygdrift.Warehouse;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Module.Refines;
using Module.Services;
using System.Threading.Tasks;

namespace Module.AppFunctions
{
    /// <summary>
    /// Visits OS2IOT and gathers all relevant data and saves it to the warehouse
    /// </summary>
    public class TimerTriggerGetDataFromApi
    {
        public TimerTriggerGetDataFromApi(ILogger<TimerTriggerGetDataFromApi> logger)
        {
            App = new AppBase<Settings>(logger);
            ApiService = new OS2IOTApiService(App);
        }

        public AppBase<Settings> App { get; }
        public OS2IOTApiService ApiService { get; }

        [FunctionName(nameof(TimerTriggerGetDataFromApi))]
        public async Task Run([TimerTrigger("%OS2IOTApiScheduleExpression%")] TimerInfo myTimer)
        {
            var organizations = App.Settings.GetOS2IOTApiOrganizationAndGateways ? await ApiService.GetOrganizationsAsync(): null;
            var applications = await ApiService.GetApplicationsAsync();
            if (applications == null)
                return;

            var models = await ApiService.GetDeviceModelsAsync();
            var iotDevices = await ApiService.GetIOTDevicesAsync(applications);
            var chirpstackGateways = App.Settings.GetOS2IOTApiOrganizationAndGateways ? await ApiService.GetChirpstackGatewaysAsync(organizations) : null;
            await ApiRefine.RefineAsync(App, true, organizations, applications, models, iotDevices, chirpstackGateways);
        }
    }
}