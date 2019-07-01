using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using IChangeFeedObserver = Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserver;
using IChangeFeedObserverFactory = Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserverFactory;

namespace Cosmonaut.WebJobs.Extensions.Trigger
{
    internal class CosmosStoreTriggerListener<T> : IListener, IChangeFeedObserverFactory
    {
        private const int ListenerNotRegistered = 0;
        private const int ListenerRegistering = 1;
        private const int ListenerRegistered = 2;

        private readonly ITriggeredFunctionExecutor _executor;
        private readonly ILogger _logger;
        private readonly DocumentCollectionInfo _monitorCollection;
        private readonly DocumentCollectionInfo _leaseCollection;
        private readonly string _hostName;
        private readonly ChangeFeedProcessorOptions _processorOptions;
        private IChangeFeedProcessor _host;
        private ChangeFeedProcessorBuilder _hostBuilder;
        private readonly IDocumentClient _monitoredDocumentClient;
        private readonly IDocumentClient _leaseDocumentClient;
        private int _listenerStatus;

        public CosmosStoreTriggerListener(ITriggeredFunctionExecutor executor, DocumentCollectionInfo documentCollectionLocation, DocumentCollectionInfo leaseCollectionLocation, ChangeFeedProcessorOptions processorOptions, IDocumentClient monitoredDocumentClient, IDocumentClient leaseDocumentClient, ILogger logger)
        {
            _logger = logger;
            _executor = executor;
            _hostName = Guid.NewGuid().ToString();
            _monitoredDocumentClient = monitoredDocumentClient;
            _leaseDocumentClient = leaseDocumentClient;
            _monitorCollection = documentCollectionLocation;
            _leaseCollection = leaseCollectionLocation;
            _processorOptions = processorOptions;
        }

        public void Cancel()
        {
            StopAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public IChangeFeedObserver CreateObserver()
        {
            return new CosmosStoreTriggerObserver<T>(_executor);
        }

        public void Dispose()
        {
            //Nothing to dispose
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var previousStatus = Interlocked.CompareExchange(ref _listenerStatus, ListenerRegistering, ListenerNotRegistered);

            if (previousStatus == ListenerRegistering)
            {
                throw new InvalidOperationException("The listener is already starting.");
            }

            if (previousStatus == ListenerRegistered)
            {
                throw new InvalidOperationException("The listener has already started.");
            }

            InitializeBuilder();

            try
            {
                await this.StartProcessorAsync();
                Interlocked.CompareExchange(ref _listenerStatus, ListenerRegistered, ListenerRegistering);
            }
            catch (Exception ex)
            {
                // Reset to NotRegistered
                _listenerStatus = ListenerNotRegistered;

                // Throw a custom error if NotFound.
                if (ex is DocumentClientException docEx && docEx.StatusCode == HttpStatusCode.NotFound)
                {
                    // Throw a custom error so that it's easier to decipher.
                    var message = $"Either the source collection '{_monitorCollection.CollectionName}' (in database '{_monitorCollection.DatabaseName}')  or the lease collection '{_leaseCollection.CollectionName}' (in database '{_leaseCollection.DatabaseName}') does not exist. Both collections must exist before the listener starts. To automatically create the lease collection, set '{nameof(CosmosStoreTriggerAttribute.CreateLeaseCollectionIfNotExists)}' to 'true'.";
                    _host = null;
                    throw new InvalidOperationException(message, ex);
                }

                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_host != null)
                {
                    await _host.StopAsync();
                    _listenerStatus = ListenerNotRegistered;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Stopping the observer failed, potentially it was never started. Exception: {ex.Message}.");
            }
        }

        internal virtual async Task StartProcessorAsync()
        {
            if (_host == null)
            {
                _host = await _hostBuilder.BuildAsync();
            }

            await _host.StartAsync();
        }

        private void InitializeBuilder()
        {
            if (_hostBuilder == null)
            {
                _hostBuilder = new ChangeFeedProcessorBuilder()
                .WithHostName(_hostName)
                .WithFeedDocumentClient((DocumentClient) _monitoredDocumentClient)
                .WithLeaseDocumentClient((DocumentClient) _leaseDocumentClient)
                .WithFeedCollection(_monitorCollection)
                .WithLeaseCollection(_leaseCollection)
                .WithProcessorOptions(_processorOptions)
                .WithObserverFactory(this);
            }
        }
    }
}
