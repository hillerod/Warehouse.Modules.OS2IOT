//using Bygdrift.Warehouse;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Extensions.Logging;
//using Module.Services;
//using System.Threading.Tasks;

//namespace Module.AppFunctions
//{
//    public class TimerTrigger
//    {
//        public TimerTrigger(ILogger<TimerTrigger> logger) => App = new AppBase<Settings>(logger);

//        public AppBase<Settings> App { get; private set; }

//        [FunctionName(nameof(TimerTriggerAsync))]
//        public async Task TimerTriggerAsync([TimerTrigger("%ScheduleExpression%")] TimerInfo myTimer)
//        {
//            App.Log.LogInformation($"The module '{App.ModuleName}' is started");
//            var data = WebService.GetData();
//            await Refines.DataRefine.Refine(App, data);
//        }
//    }
//}