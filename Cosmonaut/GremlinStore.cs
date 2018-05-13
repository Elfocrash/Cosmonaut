using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Operations;
using Cosmonaut.Storage;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Graphs;

namespace Cosmonaut
{
    public class GremlinStore : IGraphStore
    {
        public IDocumentClient DocumentClient { get; }

        public int CollectionThrouput { get; internal set; } = CosmosConstants.MinimumCosmosThroughput;

        public string CollectionName { get; private set; }

        public CosmosStoreSettings Settings { get; }

        private Database _database;
        private DocumentCollection _collection;
        private readonly IDatabaseCreator _databaseCreator;
        private readonly ICollectionCreator _collectionCreator;

        public GremlinStore(CosmosStoreSettings settings, string collectionName)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            var endpointUrl = Settings.EndpointUrl ?? throw new ArgumentNullException(nameof(Settings.EndpointUrl));
            var authKey = Settings.AuthKey ?? throw new ArgumentNullException(nameof(Settings.AuthKey));
            DocumentClient = DocumentClientFactory.CreateDocumentClient(endpointUrl, authKey);
            if (string.IsNullOrEmpty(Settings.DatabaseName)) throw new ArgumentNullException(nameof(Settings.DatabaseName));
            _collectionCreator = new CosmosCollectionCreator(DocumentClient);
            _databaseCreator = new CosmosDatabaseCreator(DocumentClient);
            CollectionName = collectionName;
            InitialiseCosmosStore();
        }

        public async Task<T> QuerySingleAsync<T>(string gremlinExpression, FeedOptions feedOptions = null, GraphSONMode graphSONMode = GraphSONMode.Compact,
            CancellationToken cancellationToken = default)
        {
            return (await DocumentClient.CreateGremlinQuery<T>(_collection, gremlinExpression, feedOptions, graphSONMode)
                .ToListAsync(cancellationToken)).SingleOrDefault();
        }

        public async Task<IEnumerable<T>> QueryMultipleAsync<T>(string gremlinExpression, FeedOptions feedOptions = null, GraphSONMode graphSONMode = GraphSONMode.Compact,
            CancellationToken cancellationToken = default)
        {
            return await DocumentClient.CreateGremlinQuery<T>(_collection, gremlinExpression, feedOptions, graphSONMode)
                .ToListAsync(cancellationToken);
        }

        public async Task ExecuteAsync(string gremlinExpression, FeedOptions feedOptions = null,
            GraphSONMode graphSONMode = GraphSONMode.Compact,
            CancellationToken cancellationToken = default)
        {
            var query = DocumentClient.CreateGremlinQuery(_collection, gremlinExpression, feedOptions, graphSONMode);

            await query.ToListAsync(cancellationToken);
        }

        public async Task<T> ExecuteSingleAsync<T>(string gremlinExpression, FeedOptions feedOptions = null,
            GraphSONMode graphSONMode = GraphSONMode.Compact, CancellationToken cancellationToken = default)
        {
            var query = DocumentClient.CreateGremlinQuery<T>(_collection, gremlinExpression, feedOptions, graphSONMode);
            return (await query.ToListAsync(cancellationToken)).SingleOrDefault();
        }

        public async Task<IEnumerable<T>> ExecuteAsync<T>(string gremlinExpression, FeedOptions feedOptions = null,
            GraphSONMode graphSONMode = GraphSONMode.Compact, CancellationToken cancellationToken = default)
        {
            var query = DocumentClient.CreateGremlinQuery<T>(_collection, gremlinExpression, feedOptions, graphSONMode);
            return await query.ToListAsync(cancellationToken);
        }

        private void InitialiseCosmosStore()
        {
            _database = GetDatabaseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            _collection = GetCollectionAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task<Database> GetDatabaseAsync()
        {
            await _databaseCreator.EnsureCreatedAsync(Settings.DatabaseName);

            var database = DocumentClient.CreateDatabaseQuery()
                .Where(db => db.Id == Settings.DatabaseName)
                .ToArray()
                .First();

            return database;
        }

        private async Task<DocumentCollection> GetCollectionAsync()
        {
            await _collectionCreator.EnsureCreatedAsync(_database, CollectionName, CollectionThrouput, Settings.IndexingPolicy);

            var collection = DocumentClient
                .CreateDocumentCollectionQuery(_database.SelfLink)
                .ToArray()
                .FirstOrDefault(c => c.Id == CollectionName);

            return collection;
        }
    }
}