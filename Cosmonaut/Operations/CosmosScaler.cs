using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Response;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Operations
{
    internal sealed class CosmosScaler<TEntity> where TEntity : class
    {
        private readonly CosmosStore<TEntity> _cosmosStore;

        public CosmosScaler(CosmosStore<TEntity> cosmosStore)
        {
            _cosmosStore = cosmosStore;
        }

        internal async Task UpscaleCollectionRequestUnitsForRequest(DocumentCollection collection, int documentCount, double operationCost)
        {
            if (!_cosmosStore.Settings.ScaleCollectionRUsAutomatically)
                return;

            if (_cosmosStore.CollectionThrouput >= documentCount * operationCost)
                return;

            var upscaleRequestUnits = (int)(Math.Round(documentCount * operationCost / 100d, 0) * 100);

            await ChangeCollectionThroughput(collection, upscaleRequestUnits >= _cosmosStore.Settings.MaximumUpscaleRequestUnits
                ? _cosmosStore.Settings.MaximumUpscaleRequestUnits
                : upscaleRequestUnits);
        }

        internal async Task DownscaleCollectionRequestUnitsToDefault(DocumentCollection collection)
        {
            if (!_cosmosStore.Settings.ScaleCollectionRUsAutomatically)
                return;

            if (!_cosmosStore.IsUpscaled)
                return;

            var throughput = typeof(TEntity).GetCollectionThroughputForEntity(_cosmosStore.Settings.DefaultCollectionThroughput);
            await ChangeCollectionThroughput(collection, throughput);
        }

        internal async Task<CosmosResponse<TEntity>> HandleUpscalingForRangeOperation(
            List<TEntity> entitiesList,
            DocumentCollection collection, 
            Func<TEntity, Task<CosmosResponse<TEntity>>> operationAsync)
        {
            var sampleEntity = entitiesList.First();
            var sampleResponse = await operationAsync(sampleEntity);

            if (!sampleResponse.IsSuccess)
                return sampleResponse;

            entitiesList.Remove(sampleEntity);
            var requestCharge = sampleResponse.ResourceResponse.RequestCharge;
            await UpscaleCollectionRequestUnitsForRequest(collection, entitiesList.Count, requestCharge);
            return sampleResponse;
        }
        
        internal async Task<CosmosMultipleResponse<TEntity>> UpscaleCollectionIfConfiguredAsSuch(List<TEntity> entitiesList, DocumentCollection collection,
            Func<TEntity, Task<CosmosResponse<TEntity>>> operationAsync)
        {
            var multipleResponse = new CosmosMultipleResponse<TEntity>();

            if (!_cosmosStore.Settings.ScaleCollectionRUsAutomatically)
                return multipleResponse;

            var sampleResponse = await HandleUpscalingForRangeOperation(entitiesList, collection, operationAsync);
            multipleResponse.AddResponse(sampleResponse);
            return multipleResponse;
        }

        private async Task ChangeCollectionThroughput(DocumentCollection collection, int requestUnits)
        {
            var collectionOffer = (OfferV2)_cosmosStore.DocumentClient.CreateOfferQuery()
                .Where(x => x.ResourceLink == collection.SelfLink).AsEnumerable().Single();
            _cosmosStore.CollectionThrouput = requestUnits;
            var replaced = await _cosmosStore.DocumentClient.ReplaceOfferAsync(new OfferV2(collectionOffer, _cosmosStore.CollectionThrouput));
            _cosmosStore.IsUpscaled = replaced.StatusCode == HttpStatusCode.OK;
        }
    }
}