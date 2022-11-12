using System.Threading.Tasks;
using Bygdrift.Warehouse;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Module.Refines;

namespace Module.AppFunctions.OS2IOT
{
    public class Os2IOT_IngestQueuedPayloads
    {
        //public Os2IOT_IngestQueuedPayloads(ILogger<Os2IOT_IngestQueuedPayloads> logger) => App = new AppBase<Settings>(logger);

        public AppBase<Settings> App { get; }

        //[FunctionName(nameof(Os2IOT_IngestQueuedPayloads))]
        //public async Task Run([TimerTrigger("%IngestQueuedPayloadsScheduleExpression%")] TimerInfo myTimer, ILogger log)
        //{
        //    App.DataLakeQueue.QueueName = "payloads";
        //    var messages = await App.DataLakeQueue.GetMessagesAsync();
        //    if (await PayloadsRefine.RefineAsync(App, messages))
        //        await App.DataLakeQueue.DeleteMessagesAsync(messages);
        //}
    }
}
