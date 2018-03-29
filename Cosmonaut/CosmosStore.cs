using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        private AsyncLazy<Database> _database;
        private AsyncLazy<DocumentCollection> _collection;

        private string _collectionName;

        public CosmosStore(CosmosStoreSettings settings)
        {
            if(settings == null)
                throw new ArgumentNullException(nameof(settings));

            var endpointUrl = settings.EndpointUrl ?? throw new ArgumentNullException(nameof(settings.DatabaseName));
            var authKey = settings.AuthKey ?? throw new ArgumentNullException(nameof(settings.AuthKey));

            DocumentClient = new DocumentClient(endpointUrl, authKey, settings.ConnectionPolicy ?? ConnectionPolicy.Default);
            _databaseName = settings.DatabaseName ?? throw new ArgumentNullException(nameof(settings.DatabaseName));
            InitialiseCosmosStore();
        }
        
        public CosmosStore(IDocumentClient documentClient, string databaseName)
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
                if (exception.Message.Contains("Resource with specified id or name already exists"))
                    return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceWithIdAlreadyExists);

                throw;
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

            foreach (var entity in entities)
            {
                var addResult = await AddAsync(entity);
                if(!addResult.IsSuccess)
                    response.FailedEntities.Add(addResult);
            }

            return response;
        }

        public async Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return (await QueryableAsync())
                .Where(predicate);
        }
        
        public async Task RemoveAsync(Func<TEntity, bool> predicate)
        {
            var documentIdsToRemove = (await QueryableAsync())
                .Where(predicate)
                .Select(GetDocumentId).ToList();

            foreach (var documentId in documentIdsToRemove)
            {
                var selfLink = GetDocumentSelfLink(documentId);
                await DocumentClient.DeleteDocumentAsync(selfLink);               
            }
        }

        public async Task<CosmosResponse<TEntity>> RemoveAsync(TEntity entity)
        {
            var documentId = GetDocumentId(entity);
            var documentSelfLink = GetDocumentSelfLink(documentId);
            var result = await DocumentClient.DeleteDocumentAsync(documentSelfLink);
            return new CosmosResponse<TEntity>(entity, result);
        }

        public async Task<CosmosResponse<TEntity>> UpdateAsync(TEntity entity)
        {
            var documentId = GetDocumentId(entity);
            var documentExists = DocumentClient.CreateDocumentQuery<Document>((await _collection).DocumentsLink)
                .Where(x => x.Id == documentId).ToList().SingleOrDefault();

            if(documentExists == null)
                return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceNotFound);

            var result = await DocumentClient.UpsertDocumentAsync((await _collection).DocumentsLink, entity);
            return new CosmosResponse<TEntity>(entity, result);
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
                if(exception.Message.Contains("Resource Not Found"))
                    return new CosmosResponse<TEntity>(CosmosOperationStatus.ResourceNotFound);

                throw;
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
                collection = new DocumentCollection { Id = _collectionName };

                collection = await DocumentClient.CreateDocumentCollectionAsync((await _database).SelfLink, collection);
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

            _database = new AsyncLazy<Database>(async () => await GetOrCreateDatabaseAsync());
            _collection = new AsyncLazy<DocumentCollection>(async () => await GetOrCreateCollectionAsync());

            PingCosmosInOrderToOpenTheClientAndPreventInitialDelay();
        }

        internal string GetDocumentSelfLink(string documentId) =>
            $"dbs/{_databaseName}/colls/{_collectionName}/docs/{documentId}/";

    }
}