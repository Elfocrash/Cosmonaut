using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public class CosmonautClient : ICosmonautClient
    {
        public CosmonautClient(IDocumentClient documentClient)
        {
            DocumentClient = documentClient;
        }

        public CosmonautClient(
            Uri endpoint, 
            string authKeyOrResourceToken, 
            ConnectionPolicy connectionPolicy = null,
            ConsistencyLevel? desiredConsistencyLevel = null)
        {
            DocumentClient = new DocumentClient(endpoint, authKeyOrResourceToken, connectionPolicy, desiredConsistencyLevel);
        }

        public CosmonautClient(
            string endpoint,
            string authKeyOrResourceToken,
            ConnectionPolicy connectionPolicy = null,
            ConsistencyLevel? desiredConsistencyLevel = null) : this(new Uri(endpoint), authKeyOrResourceToken, connectionPolicy, desiredConsistencyLevel)
        {
        }

        public async Task<Database> GetDatabaseAsync(string databaseId, RequestOptions requestOptions = null)
        {
            var databaseUri = UriFactory.CreateDatabaseUri(databaseId);
            return await DocumentClient.ReadDatabaseAsync(databaseUri, requestOptions).ExecuteCosmosQuery();
        }

        public async Task<IEnumerable<Database>> QueryDatabasesAsync(Expression<Func<Database, bool>> predicate = null, FeedOptions feedOptions = null)
        {
            if (predicate == null) predicate = x => true;
            return await DocumentClient.CreateDatabaseQuery(feedOptions).Where(predicate).ToListAsync();
        }

        public async Task<Document> GetDocumentAsync(string databaseId, string collectionId, string documentId, RequestOptions requestOptions = null)
        {
            var documentUri = UriFactory.CreateDocumentUri(databaseId, collectionId, documentId);
            return await DocumentClient.ReadDocumentAsync(documentUri, requestOptions).ExecuteCosmosQuery();
        }
        
        public async Task<IEnumerable<DocumentCollection>> QueryDocumentCollectionsAsync(string databaseId, Expression<Func<DocumentCollection, bool>> predicate = null, FeedOptions feedOptions = null)
        {
            if (predicate == null) predicate = x => true;
            var databaseUri = UriFactory.CreateDatabaseUri(databaseId);
            return await DocumentClient.CreateDocumentCollectionQuery(databaseUri, feedOptions).Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<T>> QueryDocumentsAsync<T>(string databaseId, string collectionId, Expression<Func<T, bool>> predicate = null, FeedOptions feedOptions = null)
        {
            if (predicate == null) predicate = x => true;
            var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
            return await DocumentClient.CreateDocumentQuery<T>(collectionUri, feedOptions).Where(predicate).ToGenericListAsync();
        }

        public async Task<IEnumerable<Document>> QueryDocumentsAsync(string databaseId, string collectionId, Expression<Func<Document, bool>> predicate = null, FeedOptions feedOptions = null)
        {
            if (predicate == null) predicate = x => true;
            var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
            return await DocumentClient.CreateDocumentQuery(collectionUri, feedOptions).Where(predicate).ToGenericListAsync();
        }

        public async Task<DocumentCollection> GetCollectionAsync(string databaseId, string collectionId, RequestOptions requestOptions = null)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
            return await DocumentClient.ReadDocumentCollectionAsync(collectionUri, requestOptions).ExecuteCosmosQuery();
        }

        public async Task<Offer> GetOfferForCollectionAsync(string databaseId, string collectionId, FeedOptions feedOptions = null)
        {
            var collection = await GetCollectionAsync(databaseId, collectionId);
            return await DocumentClient.CreateOfferQuery(feedOptions).SingleOrDefaultAsync(x=>x.ResourceLink == collection.SelfLink);
        }

        public async Task<OfferV2> GetOfferV2ForCollectionAsync(string databaseId, string collectionId, FeedOptions feedOptions = null)
        {
            return (OfferV2) await GetOfferForCollectionAsync(databaseId, collectionId, feedOptions);
        }

        public async Task<IEnumerable<Offer>> QueryOffersAsync(Expression<Func<Offer, bool>> predicate = null, FeedOptions feedOptions = null)
        {
            if (predicate == null) predicate = x => true;
            return await DocumentClient.CreateOfferQuery(feedOptions).Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<OfferV2>> QueryOffersV2Async(Expression<Func<Offer, bool>> predicate = null, FeedOptions feedOptions = null)
        {
            if (predicate == null) predicate = x => true;
            return (IEnumerable<OfferV2>) await QueryOffersAsync(predicate, feedOptions);
        }

        public async Task<IEnumerable<StoredProcedure>> QueryStoredProceduresAsync(string databaseId, string collectionId, Expression<Func<StoredProcedure, bool>> predicate = null, FeedOptions feedOptions = null)
        {
            if (predicate == null) predicate = x => true;
            var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
            return await DocumentClient.CreateStoredProcedureQuery(collectionUri, feedOptions).Where(predicate).ToListAsync();
        }

        public async Task<StoredProcedure> GetStoredProcedureAsync(string databaseId, string collectionId, string storedProcedureId, RequestOptions requestOptions = null)
        {
            var storedProcedureUri = UriFactory.CreateStoredProcedureUri(databaseId, collectionId, storedProcedureId);
            return await DocumentClient.ReadStoredProcedureAsync(storedProcedureUri, requestOptions).ExecuteCosmosQuery();
        }

        public async Task<IEnumerable<UserDefinedFunction>> QueryUserDefinedFunctionsAsync(string databaseId, string collectionId, Expression<Func<UserDefinedFunction, bool>> predicate = null, FeedOptions feedOptions = null)
        {
            if (predicate == null) predicate = x => true;
            var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
            return await DocumentClient.CreateUserDefinedFunctionQuery(collectionUri, feedOptions).Where(predicate).ToListAsync();
        }

        public async Task<UserDefinedFunction> GetUserDefinedFunctionAsync(string databaseId, string collectionId, string storedProcedureId, RequestOptions requestOptions = null)
        {
            var storedProcedureUri = UriFactory.CreateUserDefinedFunctionUri(databaseId, collectionId, storedProcedureId);
            return await DocumentClient.ReadUserDefinedFunctionAsync(storedProcedureUri, requestOptions).ExecuteCosmosQuery();
        }

        public IDocumentClient DocumentClient { get; }
    }
}