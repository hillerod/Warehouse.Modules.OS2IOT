using System.Threading.Tasks;
using Bygdrift.Warehouse;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Module.Refines;

namespace Module.AppFunctions
{
    public class TimerTriggerIngestQueuedPayloads
    {
        public AppBase<Settings> App { get; }

        public TimerTriggerIngestQueuedPayloads(ILogger<TimerTriggerIngestQueuedPayloads> logger)
        {
            App = new AppBase<Settings>(logger);
            App.DataLakeQueue.QueueName = "payloads";
        }

        [FunctionName(nameof(TimerTriggerIngestQueuedPayloads))]
        public async Task Run([TimerTrigger("0 */10 * * * *")] TimerInfo myTimer, ILogger log)
        {
            if (!App.Settings.IngestQueuedPayloads)
                return;

            var messages = await App.DataLakeQueue.GetMessagesAsync();
            if (await PayloadsRefine.RefineAsync(App, messages, true) != null)
                await App.DataLakeQueue.DeleteMessagesAsync(messages);
        }
    }
}
