using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public interface ICosmonautClient
    {
        IDocumentClient DocumentClient { get; }

        Task<Database> GetDatabaseAsync(string databaseId, RequestOptions requestOptions = null);

        Task<IEnumerable<Database>> QueryDatabasesAsync(Expression<Func<Database, bool>> predicate = null,
            FeedOptions feedOptions = null);

        Task<IEnumerable<DocumentCollection>> QueryDocumentCollectionsAsync(string databaseId,
            Expression<Func<DocumentCollection, bool>> predicate = null, FeedOptions feedOptions = null);

        Task<IEnumerable<T>> QueryDocumentsAsync<T>(string databaseId, string collectionId,
            Expression<Func<T, bool>> predicate = null, FeedOptions feedOptions = null);

        Task<IEnumerable<Document>> QueryDocumentsAsync(string databaseId, string collectionId,
            Expression<Func<Document, bool>> predicate = null, FeedOptions feedOptions = null);
        
        Task<Document> GetDocumentAsync(string databaseId, string collectionId, string documentId,
            RequestOptions requestOptions = null);

        Task<DocumentCollection> GetCollectionAsync(string databaseId, string collectionId,
            RequestOptions requestOptions = null);

        Task<Offer> GetOfferForCollectionAsync(string databaseId, string collectionId, FeedOptions feedOptions = null);

        Task<OfferV2> GetOfferV2ForCollectionAsync(string databaseId, string collectionId,
            FeedOptions feedOptions = null);

        Task<IEnumerable<Offer>> QueryOffersAsync(Expression<Func<Offer, bool>> predicate = null,
            FeedOptions feedOptions = null);

        Task<IEnumerable<OfferV2>> QueryOffersV2Async(Expression<Func<Offer, bool>> predicate = null,
            FeedOptions feedOptions = null);

        Task<IEnumerable<StoredProcedure>> QueryStoredProceduresAsync(string databaseId, string collectionId,
            Expression<Func<StoredProcedure, bool>> predicate = null, FeedOptions feedOptions = null);

        Task<StoredProcedure> GetStoredProcedureAsync(string databaseId, string collectionId, string storedProcedureId,
            RequestOptions requestOptions = null);

        Task<IEnumerable<UserDefinedFunction>> QueryUserDefinedFunctionsAsync(string databaseId, string collectionId,
            Expression<Func<UserDefinedFunction, bool>> predicate = null, FeedOptions feedOptions = null);

        Task<UserDefinedFunction> GetUserDefinedFunctionAsync(string databaseId, string collectionId,
            string storedProcedureId, RequestOptions requestOptions = null);
    }
}