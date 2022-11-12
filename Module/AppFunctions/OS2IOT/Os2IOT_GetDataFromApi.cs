using Bygdrift.Warehouse;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Module.Refines;
using Module.Services;
using System.Threading.Tasks;

namespace Module.AppFunctions.OS2IOT
{
    /// <summary>
    /// Visits OS2IOT and gathers all relevant data and saves it to the warehouse
    /// </summary>
    public class Os2IOT_GetDataFromApi
    {
        public Os2IOT_GetDataFromApi(ILogger<Os2IOT_GetDataFromApi> logger)
        {
            App = new AppBase<Settings>(logger);
            ApiService = new OS2IOTApiService(App);
        }

        public AppBase<Settings> App { get; }
        public OS2IOTApiService ApiService { get; }

        [FunctionName(nameof(Os2IOT_GetDataFromApi))]
        public async Task Run([TimerTrigger("%OS2IOTApiScheduleExpression%")] TimerInfo myTimer)
        {
            var organizations = await ApiService.GetOrganizationsAsync();
            var applications = await ApiService.GetApplicationsAsync();
            if (applications == null)
                return;

            var models = await ApiService.GetDeviceModelsAsync();
            var iotDevices = await ApiService.GetIOTDevicesAsync(applications);
            var chirpstackGateways = await ApiService.GetChirpstackGatewaysAsync(organizations);
            await ApiRefine.RefineAsync(App, organizations, applications, models, iotDevices, chirpstackGateways);
        }
    }
}