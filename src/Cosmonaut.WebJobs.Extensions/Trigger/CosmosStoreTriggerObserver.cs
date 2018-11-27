using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.WebJobs.Host.Executors;
using Newtonsoft.Json;

namespace Cosmonaut.WebJobs.Extensions.Trigger
{
    internal class CosmosStoreTriggerObserver<T> : IChangeFeedObserver
    {
        private readonly ITriggeredFunctionExecutor executor;

        public CosmosStoreTriggerObserver(ITriggeredFunctionExecutor executor)
        {
            this.executor = executor;
        }

        public Task CloseAsync(ChangeFeedObserverContext context, ChangeFeedObserverCloseReason reason)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Missing observer context");
            }
            return Task.CompletedTask;
        }

        public Task OpenAsync(ChangeFeedObserverContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Missing observer context");
            }
            return Task.CompletedTask;
        }

        public Task ProcessChangesAsync(ChangeFeedObserverContext context, IReadOnlyList<Document> docs)
        {
            //TODO do something better here
            var entityList = JsonConvert.DeserializeObject<IReadOnlyList<T>>(JsonConvert.SerializeObject(docs));
            return executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = entityList }, CancellationToken.None);
        }
    }
}
