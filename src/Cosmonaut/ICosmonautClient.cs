using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut
{
    public interface ICosmonautClient
    {
        CosmosClient CosmosClient { get; }

        Task<DatabaseResponse> GetDatabaseAsync(string databaseId, RequestOptions requestOptions = null);

        Task<IEnumerable<DatabaseResponse>> QueryDatabasesAsync(Expression<Func<DatabaseResponse, bool>> predicate = null,
            RequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<ContainerResponse>> QueryContainersAsync(string databaseId,
            Expression<Func<ContainerResponse, bool>> predicate = null, ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> QueryItemsAsync<T>(string databaseId, string collectionId,
            Expression<Func<T, bool>> predicate = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);

//        Task<IEnumerable<Document>> QueryDocumentsAsync(string databaseId, string collectionId,
//            Expression<Func<Document, bool>> predicate = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> QueryItemsAsync<T>(string databaseId, string collectionId,
            string sql, object parameters = null, ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> QueryItemsAsync<T>(string databaseId, string collectionId,
            string sql, IDictionary<string, object> parameters, ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default);

//        Task<Document> GetDocumentAsync(string databaseId, string collectionId, string documentId,
//            RequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        Task<T> GetItemAsync<T>(string databaseId, string collectionId, string documentId,
            ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default) where T : class;

        Task<ContainerResponse> GetContainerAsync(string databaseId, string containerId,
            ContainerRequestOptions requestOptions = null);

        IQueryable<T> Query<T>(string databaseId, string collectionId, ItemRequestOptions requestOptions = null);

        IQueryable<T> Query<T>(string databaseId, string collectionId, string sql, object parameters = null,
            ItemRequestOptions requestOptions = null);

//        IQueryable<T> Query<T>(string databaseId, string collectionId, string sql,
//            IDictionary<string, object> parameters, FeedOptions feedOptions = null);
        
//        Task<ResourceResponse<DocumentCollection>> CreateCollectionAsync(string databaseId, DocumentCollection collection,
//            RequestOptions requestOptions = null);
//
//        Task<ResourceResponse<Database>> CreateDatabaseAsync(Database database, RequestOptions requestOptions = null);
//
//        Task<ResourceResponse<Document>> CreateDocumentAsync(string databaseId, string collectionId,
//            Document document, RequestOptions requestOptions = null, CancellationToken cancellationToken = default);
//
//        Task<CosmosResponse<T>> CreateDocumentAsync<T>(string databaseId, string collectionId, T document,
//            RequestOptions requestOptions = null, CancellationToken cancellationToken = default) where T : class;
//
//        Task<ResourceResponse<Document>> DeleteDocumentAsync(string databaseId, string collectionId, string documentId,
//            RequestOptions requestOptions = null, CancellationToken cancellationToken = default);
//
//        Task<ResourceResponse<Document>> UpdateDocumentAsync(string databaseId, string collectionId, Document document,
//            RequestOptions requestOptions = null, CancellationToken cancellationToken = default);
//
//        Task<CosmosResponse<T>> UpdateDocumentAsync<T>(string databaseId, string collectionId, T document,
//            RequestOptions requestOptions = null, CancellationToken cancellationToken = default) where T : class;
//
//        Task<ResourceResponse<Document>> UpsertDocumentAsync(string databaseId, string collectionId, Document document,
//            RequestOptions requestOptions = null, CancellationToken cancellationToken = default);
//
//        Task<CosmosResponse<T>> UpsertDocumentAsync<T>(string databaseId, string collectionId, T document,
//            RequestOptions requestOptions = null, CancellationToken cancellationToken = default) where T : class;
//
//        Task<ResourceResponse<Database>> DeleteDatabaseAsync(string databaseId, RequestOptions options = null);
//
//        Task<ResourceResponse<DocumentCollection>> DeleteCollectionAsync(string databaseId, string collectionId, 
//            RequestOptions requestOptions = null);
//
//        Task<ResourceResponse<DocumentCollection>> UpdateCollectionAsync(string databaseId, string collectionId, DocumentCollection documentCollection, 
//            RequestOptions requestOptions = null);
//
//        Task<StoredProcedureResponse<TValue>> ExecuteStoredProcedureAsync<TValue>(string databaseId, string collectionId, string storedProcedureId, 
//            params object[] procedureParams);
//
//        Task<StoredProcedureResponse<TValue>> ExecuteStoredProcedureAsync<TValue>(string databaseId, string collectionId, string storedProcedureId,
//            RequestOptions requestOptions, params object[] procedureParams);
//
//        Task<StoredProcedureResponse<TValue>> ExecuteStoredProcedureAsync<TValue>(string databaseId, string collectionId, string storedProcedureId,
//            RequestOptions requestOptions, CancellationToken cancellationToken, params object[] procedureParams);
//            
//        Task<Offer> GetOfferForCollectionAsync(string databaseId, string collectionId, 
//            FeedOptions feedOptions = null, CancellationToken cancellationToken = default);
//
//        Task<OfferV2> GetOfferV2ForCollectionAsync(string databaseId, string collectionId,
//            FeedOptions feedOptions = null, CancellationToken cancellationToken = default);
//
//        Task<Offer> GetOfferForDatabaseAsync(string databaseId, FeedOptions feedOptions = null,
//            CancellationToken cancellationToken = default);
//
//        Task<OfferV2> GetOfferV2ForDatabaseAsync(string databaseId, FeedOptions feedOptions = null,
//            CancellationToken cancellationToken = default);
//
//        Task<IEnumerable<Offer>> QueryOffersAsync(Expression<Func<Offer, bool>> predicate = null,
//            FeedOptions feedOptions = null, CancellationToken cancellationToken = default);
//
//        Task<IEnumerable<OfferV2>> QueryOffersV2Async(Expression<Func<Offer, bool>> predicate = null,
//            FeedOptions feedOptions = null, CancellationToken cancellationToken = default);
//
//        Task<ResourceResponse<Offer>> UpdateOfferAsync(Offer offer);
//
//        Task<IEnumerable<StoredProcedure>> QueryStoredProceduresAsync(string databaseId, string collectionId,
//            Expression<Func<StoredProcedure, bool>> predicate = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);
//
//        Task<StoredProcedure> GetStoredProcedureAsync(string databaseId, string collectionId, string storedProcedureId,
//            RequestOptions requestOptions = null);
    }
}