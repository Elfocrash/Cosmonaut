using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;

namespace Cosmonaut.WebJobs.Extensions.Trigger
{
    internal class CosmosStoreTriggerBinding<T> : ITriggerBinding where T : class
    {
        private readonly ParameterInfo _parameter;
        private readonly ILogger _logger;
        private readonly IDocumentClient _monitoredDocumentClient;
        private readonly IDocumentClient _leaseDocumentClient;
        private readonly IReadOnlyDictionary<string, object> _emptyBindingData = new Dictionary<string, object>();

        public CosmosStoreTriggerBinding(ParameterInfo parameter, DocumentCollectionInfo documentCollectionLocation, DocumentCollectionInfo leaseCollectionLocation, IDocumentClient monitoredDocumentClient, IDocumentClient leaseDocumentClient, ChangeFeedProcessorOptions processorOptions, ILogger logger)
        {
            DocumentCollectionLocation = documentCollectionLocation;
            LeaseCollectionLocation = leaseCollectionLocation;
            _monitoredDocumentClient = monitoredDocumentClient;
            _leaseDocumentClient = leaseDocumentClient;
            _parameter = parameter;
            ChangeFeedProcessorOptions = processorOptions;
            _logger = logger;
        }

        public Type TriggerValueType => typeof(IReadOnlyList<T>);

        internal DocumentCollectionInfo DocumentCollectionLocation { get; }

        internal DocumentCollectionInfo LeaseCollectionLocation { get; }

        internal ChangeFeedProcessorOptions ChangeFeedProcessorOptions { get; }

        public IReadOnlyDictionary<string, Type> BindingDataContract { get; } = new Dictionary<string, Type>();

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            return Task.FromResult<ITriggerData>(new TriggerData(null, _emptyBindingData));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Missing listener context");
            }

            return Task.FromResult<IListener>(new CosmosStoreTriggerListener<T>(context.Executor, DocumentCollectionLocation, LeaseCollectionLocation, ChangeFeedProcessorOptions, _monitoredDocumentClient, _leaseDocumentClient, _logger));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new CosmosStoreTriggerParameterDescriptor
            {
                Name = _parameter.Name,
                Type = CosmosStoreTriggerConstants.TriggerName,
                CollectionName = DocumentCollectionLocation.CollectionName
            };
        }
    }
}