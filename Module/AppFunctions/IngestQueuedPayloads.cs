using System.Threading.Tasks;
using Bygdrift.Warehouse;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Module.Refines;

namespace Module.AppFunctions
{
    public class IngestQueuedPayloads
    {
        public IngestQueuedPayloads(ILogger<GetDataFromApi> logger) => App = new AppBase<Settings>(logger);

        public AppBase<Settings> App { get; }

        [FunctionName(nameof(IngestQueuedPayloads))]
        public async Task Run([TimerTrigger("%IngestQueuedPayloadsScheduleExpression%")]TimerInfo myTimer, ILogger log)
        {
            App.DataLakeQueue.QueueName = "payloads";
            var messages = await App.DataLakeQueue.GetMessagesAsync();
            if(await PayloadsRefine.RefineAsync(App, messages))
                await App.DataLakeQueue.DeleteMessagesAsync(messages);
        }
    }
}
