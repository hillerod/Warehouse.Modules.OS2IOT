using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.AppFunctions.Helpers.Models
{
    public class QueueResponse
    {
        public QueueResponse(Azure.Storage.Queues.Models.QueueMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var decodedBodyBase64 = Encoding.UTF8.GetString(message.Body);
            Body = decodedBodyBase64;
            //Body = Encoding.UTF8.GetString(Convert.FromBase64String(decodedBodyBase64));

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
