//using Bygdrift.Warehouse;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Extensions.Logging;
//using Module.Refines;
//using Module.Services;
//using System.Threading.Tasks;

//namespace Module.AppFunctions
//{
//    public class DataFromApi
//    {
//        public DataFromApi(ILogger<DataFromApi> logger)
//        {
//            App = new AppBase<Settings>(logger);
//            ApiService = new ApiService(App);
//        }

//        public AppBase<Settings> App { get; }
//        public ApiService ApiService { get; }

//        [FunctionName(nameof(GetDataFromApi))]
//        public async Task GetDataFromApi([TimerTrigger("%OS2IOTApiScheduleExpression%")] TimerInfo myTimer)
//        {
//            var applications = await ApiService.GetApplicationsAsync();
//            var models = await ApiService.GetDeviceModelsAsync();
//            var iotDevices = await ApiService.GetIOTDevicesAsync(applications);
//            await ApiRefine.Refine(App, applications, models, iotDevices);
//        }
//    }
//}