using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Cosmonaut.Attributes;
using Cosmonaut.Exceptions;
using Cosmonaut.Extensions;
using Cosmonaut.Response;
using Humanizer;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Cosmonaut
{
    public class CosmosStore<TEntity> : ICosmosStore<TEntity> where TEntity : class
    {
        private readonly string _databaseName;
        private int _collectionThrouput = CosmosStoreSettings.DefaultCollectionThroughput;
        private AsyncLazy<Database> _database;
        private AsyncLazy<DocumentCollection> _collection;
        public readonly CosmosStoreSettings Settings;
        private string _collectionName;

        public CosmosStore(CosmosStoreSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            var endpointUrl = Settings.EndpointUrl ?? throw new ArgumentNullException(nameof(Settings.DatabaseName));
            var authKey = Settings.AuthKey ?? throw new ArgumentNullException(nameof(Settings.AuthKey));
            DocumentClient = new DocumentClient(endpointUrl, authKey, Settings.ConnectionPolicy ?? ConnectionPolicy.Default);
            _databaseName = Settings.DatabaseName ?? throw new ArgumentNullException(nameof(Settings.DatabaseName));
            InitialiseCosmosStore();
        }
        
        internal CosmosStore(IDocumentClient documentClient, string databaseName)
        {
            DocumentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
            _databaseName = databaseName ?? throw new ArgumentNullException(nameof(documentClient));
            InitialiseCosmosStore();
        }
        
        public async Task<CosmosResponse<TEntity>> AddAsync(TEntity entity, RequestOptions requestOptions = null)
        {
            var collection = await _collection;
            var safeDocument = GetCosmosDbFriendlyEntity(entity);

            try
            {
                ResourceResponse<Document> addedDocument =
                    await DocumentClient.CreateDocumentAsync(collection.SelfLink, safeDocument, requestOptions);
                return new CosmosResponse<TEntity>(entity, addedDocument);
            }
            catch (DocumentClientException exception)
            {
                return HandleDocumentClientException(entity, exception);
            }
        }

        public async Task<CosmosMultipleReponse<TEntity>> AddRangeAsync(params TEntity[] entities)
        {
            return await AddRangeAsync((IEnumerable<TEntity>)entities);
        }

        public async Task<CosmosMultipleReponse<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var response = new CosmosMultipleReponse<TEntity>();
            var addEntitiesTasks = entities.Select(entity => AddAsync(entity));
            var results = (await Task.WhenAll(addEntitiesTasks)).ToList();

            async Task RetryPotentialRateLimitFailures()
            {
                var failedBecauseOfRateLimit =
                    results.Where(x => x.CosmosOperationStatus == CosmosOperationStatus.RequestRateIsLarge).ToList();
                if (!failedBecauseOfRateLimit.Any())
                    return;

                results.RemoveAll(x => x.CosmosOperationStatus == CosmosOperationStatus.RequestRateIsLarge);
                addEntitiesTasks = failedBecauseOfRateLimit.Select(entity => AddAsync(entity.Entity));
                results.AddRange(await Task.WhenAll(addEntitiesTasks));
            }

            await RetryPotentialRateLimitFailures();
            response.FailedEntities.AddRange(results.Where(x=> !x.IsSuccess));
            return response;
        }

        public async Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return (await QueryableAsync())
                .Where(predicate);
        }
        
        public async Task<CosmosMultipleReponse<TEntity>> RemoveAsync(Func<TEntity, bool> predicate)
        {
            var documentIdsToRemove = await ToListAsync(predicate);
            var response = new CosmosMultipleReponse<TEntity>();

            var removeEntitiesTasks = documentIdsToRemove.Select(RemoveAsync);
            var results = (await Task.WhenAll(removeEntitiesTasks)).ToList();

            async Task RetryPotentialRateLimitFailures()
            {
                var failedBecauseOfRateLimit =
                    results.Where(x => x.CosmosOperationStatus == CosmosOperationStatus.RequestRateIsLarge).ToList();
                if (!failedBecauseOfRateLimit.Any())
                    return;

                results.RemoveAll(x => x.CosmosOperationStatus == CosmosOperationStatus.RequestRateIsLarge);
                removeEntitiesTasks = failedBecauseOfRateLimit.Select(entity => RemoveAsync(entity.Entity));
                results.AddRange(await Task.WhenAll(removeEntitiesTasks));
            }

            await RetryPotentialRateLimitFailures();
            response.FailedEntities.AddRange(results.Where(x => !x.IsSuccess));
            return response;
        }

        public async Task<CosmosResponse<TEntity>> RemoveAsync(TEntity entity)
        {
            try
            {
                var documentId = GetDocumentId(entity);
                var documentSelfLink = GetDocumentSelfLink(documentId);
                var result = await DocumentClient.DeleteDocumentAsync(documentSelfLink);
                return new CosmosResponse<TEntity>(entity, result);
            }
            catch (DocumentClientException exception)
            {
                return HandleDocumentClientException(entity, exception);
            }
        }

        public async Task<CosmosResponse<TEntity>> UpdateAsync(TEntity entity)
        {
            try
            {
                var documentId = GetDocumentId(entity);
                var documentExists = DocumentClient.CreateDocumentQuery<Document>((await _collection).DocumentsLink)
                    .Where(x => x.Id == documentId).ToList().SingleOrDefault();

                if (documentExists == null)
                    return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceNotFound);

                var result = await DocumentClient.UpsertDocumentAsync((await _collection).DocumentsLink, entity);
                return new CosmosResponse<TEntity>(entity, result);
            }
            catch (DocumentClientException exception)
            {
                return HandleDocumentClientException(exception);
            }
        }
        
        public async Task<CosmosResponse<TEntity>> RemoveByIdAsync(string id)
        {
            var documentSelfLink = GetDocumentSelfLink(id);
            try
            {
                var result = await DocumentClient.DeleteDocumentAsync(documentSelfLink);
                return new CosmosResponse<TEntity>(result);
            }
            catch (DocumentClientException exception)
            {
                return HandleDocumentClientException(exception);
            }
        }

        public async Task<List<TEntity>> ToListAsync(Func<TEntity, bool> predicate = null)
        {
            if (predicate == null)
                predicate = entity => true;

            return DocumentClient.CreateDocumentQuery<TEntity>((await _collection).DocumentsLink)
                .Where(predicate)
                .ToList();
        }

        public async Task<TEntity> FirstOrDefaultAsync(Func<TEntity, bool> predicate)
        {
            return
                (await QueryableAsync())
                    .FirstOrDefault(predicate);
        }
        
        public IDocumentClient DocumentClient { get; }

        public async Task<IOrderedQueryable<TEntity>> QueryableAsync()
        {
            return DocumentClient.CreateDocumentQuery<TEntity>((await _collection).DocumentsLink);
        }

        internal async Task<Database> GetOrCreateDatabaseAsync()
        {
            Database database = DocumentClient.CreateDatabaseQuery()
                .Where(db => db.Id == _databaseName)
                .ToArray()
                .FirstOrDefault();

            if (database == null)
            {
                database = await DocumentClient.CreateDatabaseAsync(
                    new Database { Id = _databaseName });
            }

            return database;
        }

        internal string GetDocumentId(TEntity entity)
        {
            var propertyInfos = entity.GetType().GetProperties();

            var propertyWithJsonPropertyId =
                propertyInfos.SingleOrDefault(x => x.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName == "id");

            if (propertyWithJsonPropertyId != null && !string.IsNullOrEmpty(propertyWithJsonPropertyId.GetValue(entity)?.ToString()))
                return propertyWithJsonPropertyId.GetValue(entity).ToString();

            var propertyNamedId = propertyInfos.SingleOrDefault(x => x.Name.Equals("id", StringComparison.OrdinalIgnoreCase));

            if (propertyNamedId != null && !string.IsNullOrEmpty(propertyNamedId.GetValue(entity)?.ToString()))
                return propertyNamedId.GetValue(entity).ToString();

            var potentialCosmosEntityId = entity.GetType().GetInterface(nameof(ICosmosEntity))
                .GetProperties().SingleOrDefault(x =>
                    x.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName == "id");

            if (potentialCosmosEntityId != null && !string.IsNullOrEmpty(potentialCosmosEntityId.GetValue(entity)?.ToString()))
                return potentialCosmosEntityId.GetValue(entity).ToString();

            throw new CosmosEntityWithoutIdException<TEntity>(entity);
        }

        internal async Task<DocumentCollection> GetOrCreateCollectionAsync()
        {
            var collection = DocumentClient
                .CreateDocumentCollectionQuery((await _database).SelfLink)
                .ToArray()
                .FirstOrDefault(c => c.Id == _collectionName);
            
            if (collection == null)
            {
                collection = new DocumentCollection { Id = _collectionName};

                collection = await DocumentClient.CreateDocumentCollectionAsync((await _database).SelfLink, collection, new RequestOptions
                {
                    OfferThroughput = _collectionThrouput
                });

                return collection;
            }

            var collectionOffer = (OfferV2)DocumentClient.CreateOfferQuery().Where(x => x.ResourceLink == collection.SelfLink).AsEnumerable().Single();
            var currentOfferThroughput = collectionOffer.Content.OfferThroughput;
            if (_collectionThrouput != currentOfferThroughput)
            {
                var updated = await DocumentClient.ReplaceOfferAsync(new OfferV2(collectionOffer, _collectionThrouput));
                if (updated.StatusCode != HttpStatusCode.OK)
                    throw new CosmosCollectionThroughputUpdateException(collection);
            }

            return collection;
        }

        internal dynamic GetCosmosDbFriendlyEntity(TEntity entity)
        {
            var propertyInfos = entity.GetType().GetProperties();

            var containsJsonAttributeIdCount =
                propertyInfos.Count(x => x.GetCustomAttributes().ToList().Contains(new JsonPropertyAttribute("id")))
                + entity.GetType().GetInterfaces().Count(x=> x.GetProperties()
                .Any(prop => prop.GetCustomAttributes<JsonPropertyAttribute>()
                .Any(attr => attr.PropertyName.Equals("id", StringComparison.OrdinalIgnoreCase))));

            if (containsJsonAttributeIdCount > 1)
                throw new ArgumentException("An entity can only have one cosmos db id. Only one [JsonAttribute(\"id\")] allowed per entity.");

            var idProperty = propertyInfos.FirstOrDefault(x =>
                x.Name.Equals("id", StringComparison.OrdinalIgnoreCase) && x.PropertyType == typeof(string));

            if (idProperty != null && containsJsonAttributeIdCount == 1)
            {
                if (!idProperty.GetCustomAttributes<JsonPropertyAttribute>().Any(x =>
                    x.PropertyName.Equals("id", StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException(
                        "An entity can only have one cosmos db id. Either rename the Id property or remove the [JsonAttribute(\"id\")].");
                return entity;
            }

            if (idProperty == null || containsJsonAttributeIdCount == 1)
                return entity;

            if(idProperty.GetValue(entity) == null)
                idProperty.SetValue(entity, Guid.NewGuid().ToString());

            //TODO Clean this up. It is a very bad hack
            dynamic mapped = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(entity));

            SetTheCosmosDbIdBasedOnTheObjectIndex(entity, mapped);
            
            RemovePotentialDuplicateIdProperties(mapped);

            return mapped;
        }

        internal void PingCosmosInOrderToOpenTheClientAndPreventInitialDelay()
        {
            DocumentClient.ReadDatabaseAsync(_database.GetAwaiter().GetResult().SelfLink);
        }

        internal void SetTheCosmosDbIdBasedOnTheObjectIndex(TEntity entity, dynamic mapped)
        {
            mapped.id = GetDocumentId(entity);
        }

        internal string GetCollectionNameForEntity()
        {
            var collectionNameAttribute = typeof(TEntity).GetCustomAttribute<CosmosCollectionAttribute>();
            
            var collectionName = collectionNameAttribute?.Name;

            return !string.IsNullOrEmpty(collectionName) ? collectionName : typeof(TEntity).Name.ToLower().Pluralize();
        }

        internal int GetCollectionThroughputForEntity()
        {
            var collectionNameAttribute = typeof(TEntity).GetCustomAttribute<CosmosCollectionAttribute>();

            var throughput = collectionNameAttribute != null && collectionNameAttribute.Throughput != -1 ? collectionNameAttribute.Throughput : Settings.CollectionThroughput;

            if (throughput < 400 || throughput > 10000)
                throw new IllegalCosmosThroughputException();

            return throughput;
        }

        internal static void RemovePotentialDuplicateIdProperties(dynamic mapped)
        {
            if (mapped.Id != null)
                mapped.Remove("Id");

            if (mapped.ID != null)
                mapped.Remove("ID");

            if (mapped.iD != null)
                mapped.Remove("iD");
        }
        
        internal void InitialiseCosmosStore()
        {
            _collectionName = GetCollectionNameForEntity();
            _collectionThrouput = GetCollectionThroughputForEntity();

            _database = new AsyncLazy<Database>(async () => await GetOrCreateDatabaseAsync());
            _collection = new AsyncLazy<DocumentCollection>(async () => await GetOrCreateCollectionAsync());

            PingCosmosInOrderToOpenTheClientAndPreventInitialDelay();
        }

        internal CosmosResponse<TEntity> HandleDocumentClientException(TEntity entity, DocumentClientException exception)
        {
            if (exception.Message.Contains("Resource Not Found"))
                return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceNotFound);

            if (exception.Message.Contains("Request rate is large"))
                return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.RequestRateIsLarge);

            if (exception.Message.Contains("Resource with specified id or name already exists"))
                return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceWithIdAlreadyExists);

            throw exception;
        }

        internal CosmosResponse<TEntity> HandleDocumentClientException(DocumentClientException exception)
        {
            return HandleDocumentClientException(null, exception);
        }

        internal string GetDocumentSelfLink(string documentId) =>
            $"dbs/{_databaseName}/colls/{_collectionName}/docs/{documentId}/";

    }
}