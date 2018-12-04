using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.WebJobs.Extensions.Config
{
    public class CosmosStoreBindingOptions
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public ConnectionMode? ConnectionMode { get; set; }

        public Protocol? Protocol { get; set; }

        public ChangeFeedHostOptions LeaseOptions { get; set; } = new ChangeFeedHostOptions();
    }
}