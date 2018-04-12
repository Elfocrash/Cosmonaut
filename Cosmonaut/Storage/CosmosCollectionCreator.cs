using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Storage
{
    internal class CosmosCollectionCreator<TEntity> : ICollectionCreator<TEntity> where TEntity : class
    {
        private readonly IDocumentClient _documentClient;
        private readonly CosmosDocumentProcessor<TEntity> _documentProcessor;

        public CosmosCollectionCreator(IDocumentClient documentClient, 
            CosmosDocumentProcessor<TEntity> documentProcessor)
        {
            _documentClient = documentClient;
            _documentProcessor = documentProcessor;
        }

        public async Task<bool> EnsureCreatedAsync(Type entityType, 
            Database database, 
            int collectionThroughput)
        {
            var collectionName = typeof(TEntity).GetCollectionName();
            var collection = _documentClient
                .CreateDocumentCollectionQuery(database.SelfLink)
                .ToArray()
                .FirstOrDefault(c => c.Id == collectionName);

            if (collection != null)
                return true;

            collection = new DocumentCollection
            {
                Id = collectionName
            };
            var partitionKey = _documentProcessor.GetPartitionKeyForEntity(typeof(TEntity));

            if (partitionKey != null)
                collection.PartitionKey = _documentProcessor.GetPartitionKeyForEntity(typeof(TEntity));

            collection = await _documentClient.CreateDocumentCollectionAsync(database.SelfLink, collection, new RequestOptions
            {
                OfferThroughput = collectionThroughput
            });

            return collection != null;
        }
    }
}