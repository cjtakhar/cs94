using Azure.Core;
using Azure.Storage.Queues;

namespace ManagedIdentityQueueWebApi
{
    public class JsonQueueClient : QueueClient
    {
        public JsonQueueClient(Uri queueUri, TokenCredential credential, QueueClientOptions options = default) : base(queueUri, credential, options)
        {
        }

    }
}
