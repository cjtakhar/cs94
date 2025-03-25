using Azure.Core;
using Azure.Storage.Queues;

namespace ManagedIdentityQueueWebApi
{
    public class StringQueueClient : QueueClient
    {
        public StringQueueClient(Uri queueUri, TokenCredential credential, QueueClientOptions options = default) : base(queueUri, credential, options)
        {
        }

    }
}
