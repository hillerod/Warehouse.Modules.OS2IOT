using System;
using System.Text;

namespace Module.AppFunctions.Models
{
    public class QueueResponse
    {
        public QueueResponse(Azure.Storage.Queues.Models.QueueMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Body = Encoding.UTF8.GetString(message.Body);
            Created = message.InsertedOn;
            Expires = message.ExpiresOn;
            Id = message.MessageId;
        }

        public QueueResponse(Azure.Storage.Queues.Models.PeekedMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Body = Encoding.UTF8.GetString(message.Body);
            Created = message.InsertedOn;
            Expires = message.ExpiresOn;
            Id = message.MessageId;
        }

        public string Body { get; set; }
        public string Id { get; set; }
        public DateTimeOffset? Created { get; set; }
        public DateTimeOffset? Expires { get; set; }
    }
}
