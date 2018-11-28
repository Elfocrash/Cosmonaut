using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.WebJobs.Host.Executors;
using Newtonsoft.Json;

namespace Cosmonaut.WebJobs.Extensions.Trigger
{
    internal class CosmosStoreTriggerObserver<T> : IChangeFeedObserver
    {
        private readonly ITriggeredFunctionExecutor _executor;

        public CosmosStoreTriggerObserver(ITriggeredFunctionExecutor executor)
        {
            _executor = executor;
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
            var entityType = typeof(T);
            var isSharedCollection = entityType.UsesSharedCollection();
            var sharedCollectionEntityName = isSharedCollection ? entityType.GetSharedCollectionEntityName() : string.Empty;

            if (string.IsNullOrEmpty(sharedCollectionEntityName))
            {
                return _executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = ConvertDocsToObjects(docs) }, CancellationToken.None);
            }

            var docsToProcess = docs.Where(doc =>
            {
                var cosmosEntityName = doc.GetPropertyValue<string>(nameof(ISharedCosmosEntity.CosmosEntityName));
                return !string.IsNullOrEmpty(cosmosEntityName) &&
                       sharedCollectionEntityName.Equals(cosmosEntityName);
            }).ToList();

            if (!docsToProcess.Any())
                return Task.CompletedTask;

            return _executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = ConvertDocsToObjects(docsToProcess) }, CancellationToken.None);
        }

        private static IReadOnlyList<T> ConvertDocsToObjects(IReadOnlyList<Document> docs)
        {
            return JsonConvert.DeserializeObject<IReadOnlyList<T>>(JsonConvert.SerializeObject(docs));
        }
    }
}
