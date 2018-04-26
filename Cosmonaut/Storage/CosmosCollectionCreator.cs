using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Storage
{
    public class CosmosCollectionCreator : ICollectionCreator
    {
        private readonly IDocumentClient _documentClient;

        public CosmosCollectionCreator(IDocumentClient documentClient)
        {
            _documentClient = documentClient;
        }

        public async Task<bool> EnsureCreatedAsync(Type entityType, 
            Database database, 
            int collectionThroughput,
            IndexingPolicy indexingPolicy = null)
        {
            var isSharedCollection = entityType.UsesSharedCollection();

            var collectionName = isSharedCollection ? entityType.GetSharedCollectionName() : entityType.GetCollectionName();

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

            SetPartitionKeyIsCollectionIsNotShared(entityType, isSharedCollection, collection);
            SetPartitionKeyAsIdIfCollectionIsShared(isSharedCollection, collection);

            if (indexingPolicy != null)
                collection.IndexingPolicy = indexingPolicy;

            //SetDefaultIndexingPolicyForSharedCollection(isSharedCollection, collection);

            collection = await _documentClient.CreateDocumentCollectionAsync(database.SelfLink, collection, new RequestOptions
            {
                OfferThroughput = collectionThroughput
            });

            return collection != null;
        }

        private static void SetDefaultIndexingPolicyForSharedCollection(bool isSharedCollection, DocumentCollection collection)
        {
            if (isSharedCollection)
            {
                collection.IndexingPolicy.IncludedPaths.Add(new IncludedPath
                {
                    Path = "/id",
                    Indexes = new Collection<Index>
                    {
                        new RangeIndex(DataType.String, -1)
                    }
                });
            }
        }

        private static void SetPartitionKeyAsIdIfCollectionIsShared(bool isSharedCollection, DocumentCollection collection)
        {
            if (isSharedCollection)
            {
                collection.PartitionKey = DocumentHelpers.GetPartitionKeyDefinition(CosmosConstants.CosmosId);
            }
        }

        private static void SetPartitionKeyIsCollectionIsNotShared(Type entityType, bool isSharedCollection, DocumentCollection collection)
        {
            if (isSharedCollection) return;
            var partitionKey = entityType.GetPartitionKeyForEntity();

            if (partitionKey != null)
                collection.PartitionKey = partitionKey;
        }
    }
}