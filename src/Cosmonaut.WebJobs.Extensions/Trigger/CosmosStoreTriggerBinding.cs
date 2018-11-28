using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.Client;
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
        private readonly IReadOnlyDictionary<string, object> _emptyBindingData = new Dictionary<string, object>();

        public CosmosStoreTriggerBinding(ParameterInfo parameter, DocumentCollectionInfo documentCollectionLocation, DocumentCollectionInfo leaseCollectionLocation, ChangeFeedHostOptions leaseHostOptions, ChangeFeedOptions changeFeedOptions, ILogger logger)
        {
            DocumentCollectionLocation = documentCollectionLocation;
            LeaseCollectionLocation = leaseCollectionLocation;
            ChangeFeedHostOptions = leaseHostOptions;
            _parameter = parameter;
            _logger = logger;
            ChangeFeedOptions = changeFeedOptions;
        }

        public Type TriggerValueType => typeof(IReadOnlyList<T>);

        internal DocumentCollectionInfo DocumentCollectionLocation { get; }

        internal DocumentCollectionInfo LeaseCollectionLocation { get; }

        internal ChangeFeedHostOptions ChangeFeedHostOptions { get; }

        internal ChangeFeedOptions ChangeFeedOptions { get; }

        public IReadOnlyDictionary<string, Type> BindingDataContract { get; } = new Dictionary<string, Type>();

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            return Task.FromResult<ITriggerData>(new TriggerData(null, _emptyBindingData));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Missing listener context");
            }

            return Task.FromResult<IListener>(new CosmosStoreTriggerListener<T>(context.Executor, DocumentCollectionLocation, LeaseCollectionLocation, ChangeFeedHostOptions, ChangeFeedOptions, _logger));
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