using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Response;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public interface ICosmonautClient
    {
        IDocumentClient DocumentClient { get; }

        Task<Database> GetDatabaseAsync(string databaseId, RequestOptions requestOptions = null);

        Task<IEnumerable<Database>> QueryDatabasesAsync(Expression<Func<Database, bool>> predicate = null,
            FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<DocumentCollection>> QueryCollectionsAsync(string databaseId,
            Expression<Func<DocumentCollection, bool>> predicate = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> QueryDocumentsAsync<T>(string databaseId, string collectionId,
            Expression<Func<T, bool>> predicate = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<Document>> QueryDocumentsAsync(string databaseId, string collectionId,
            Expression<Func<Document, bool>> predicate = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> QueryDocumentsAsync<T>(string databaseId, string collectionId,
            string sql, object parameters = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> QueryDocumentsAsync<T>(string databaseId, string collectionId,
            string sql, IDictionary<string, object> parameters, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        Task<Document> GetDocumentAsync(string databaseId, string collectionId, string documentId,
            RequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        Task<T> GetDocumentAsync<T>(string databaseId, string collectionId, string documentId,
            RequestOptions requestOptions = null, CancellationToken cancellationToken = default) where T : class;

        Task<DocumentCollection> GetCollectionAsync(string databaseId, string collectionId,
            RequestOptions requestOptions = null);

        Task<Offer> GetOfferForCollectionAsync(string databaseId, string collectionId, 
            FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        Task<OfferV2> GetOfferV2ForCollectionAsync(string databaseId, string collectionId,
            FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        Task<Offer> GetOfferForDatabaseAsync(string databaseId, FeedOptions feedOptions = null,
            CancellationToken cancellationToken = default);

        Task<OfferV2> GetOfferV2ForDatabaseAsync(string databaseId, FeedOptions feedOptions = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Offer>> QueryOffersAsync(Expression<Func<Offer, bool>> predicate = null,
            FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<OfferV2>> QueryOffersV2Async(Expression<Func<Offer, bool>> predicate = null,
            FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        Task<ResourceResponse<Offer>> UpdateOfferAsync(Offer offer);

        Task<IEnumerable<StoredProcedure>> QueryStoredProceduresAsync(string databaseId, string collectionId,
            Expression<Func<StoredProcedure, bool>> predicate = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        Task<StoredProcedure> GetStoredProcedureAsync(string databaseId, string collectionId, string storedProcedureId,
            RequestOptions requestOptions = null);

        IQueryable<T> Query<T>(string databaseId, string collectionId, FeedOptions feedOptions = null);

        IQueryable<T> Query<T>(string databaseId, string collectionId, string sql, object parameters = null,
            FeedOptions feedOptions = null);

        IQueryable<T> Query<T>(string databaseId, string collectionId, string sql,
            IDictionary<string, object> parameters, FeedOptions feedOptions = null);
        
        Task<ResourceResponse<DocumentCollection>> CreateCollectionAsync(string databaseId, DocumentCollection collection,
            RequestOptions requestOptions = null);

        Task<ResourceResponse<Database>> CreateDatabaseAsync(Database database, RequestOptions requestOptions = null);

        Task<ResourceResponse<Document>> CreateDocumentAsync(string databaseId, string collectionId,
            Document document, RequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        Task<CosmosResponse<T>> CreateDocumentAsync<T>(string databaseId, string collectionId, T document,
            RequestOptions requestOptions = null, CancellationToken cancellationToken = default) where T : class;

        Task<ResourceResponse<Document>> DeleteDocumentAsync(string databaseId, string collectionId, string documentId,
            RequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        Task<ResourceResponse<Document>> UpdateDocumentAsync(string databaseId, string collectionId, Document document,
            RequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        Task<CosmosResponse<T>> UpdateDocumentAsync<T>(string databaseId, string collectionId, T document,
            RequestOptions requestOptions = null, CancellationToken cancellationToken = default) where T : class;

        Task<ResourceResponse<Document>> UpsertDocumentAsync(string databaseId, string collectionId, Document document,
            RequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        Task<CosmosResponse<T>> UpsertDocumentAsync<T>(string databaseId, string collectionId, T document,
            RequestOptions requestOptions = null, CancellationToken cancellationToken = default) where T : class;

        Task<ResourceResponse<Database>> DeleteDatabaseAsync(string databaseId, RequestOptions options = null);

        Task<ResourceResponse<DocumentCollection>> DeleteCollectionAsync(string databaseId, string collectionId, 
            RequestOptions requestOptions = null);

        Task<ResourceResponse<DocumentCollection>> UpdateCollectionAsync(string databaseId, string collectionId, DocumentCollection documentCollection, 
            RequestOptions requestOptions = null);

        Task<StoredProcedureResponse<TValue>> ExecuteStoredProcedureAsync<TValue>(string databaseId, string collectionId, string storedProcedureId, 
            params object[] procedureParams);

        Task<StoredProcedureResponse<TValue>> ExecuteStoredProcedureAsync<TValue>(string databaseId, string collectionId, string storedProcedureId,
            RequestOptions requestOptions, params object[] procedureParams);

        Task<StoredProcedureResponse<TValue>> ExecuteStoredProcedureAsync<TValue>(string databaseId, string collectionId, string storedProcedureId,
            RequestOptions requestOptions, CancellationToken cancellationToken, params object[] procedureParams);
    }
}