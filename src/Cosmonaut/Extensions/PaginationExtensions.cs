using System;
using System.Linq;
using System.Reflection;
using Cosmonaut.Internal;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut.Extensions
{
    public static class PaginationExtensions
    {
        private const string CosmosLinqQueryTypeName = "CosmosLinqQuery`1";

        /// <summary>
        /// Adds pagination for your CosmosDB query. This is an inefficient and expensive form of pagination because it goes
        /// though all the documents to get to the page you want. The usage of WithPagination with the ContinuationToken is recommended. 
        /// Read more at https://github.com/Elfocrash/Cosmonaut
        /// </summary>
        /// <returns>A specific page of the results that your query matches.</returns>
        public static FeedIterator<T> WithPagination<T>(this FeedIterator<T> iterator, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be a positive number.");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be a positive number.");
            }

            return GetQueryableWithPaginationSettings(iterator, $"{nameof(WithPagination)}/{pageNumber}", pageSize);
        }

        /// <summary>
        /// Adds pagination for your CosmosDB query. This is an efficient and cheap form of pagination because it doesn't go 
        /// though all the documents to get to the page you want. Read more at https://github.com/Elfocrash/Cosmonaut
        /// </summary>
        /// ///
        /// <param name="iterator">The DocumentQueryable for the operation</param>
        /// <param name="continuationToken">When null or empty string, the first page of items will be returned</param>
        /// <param name="pageSize">The size of the page we are expecting</param>
        /// <returns>A specific page of the results that your query matches.</returns>
        public static FeedIterator<T> WithPagination<T>(this FeedIterator<T> iterator, string continuationToken, int pageSize)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be a positive number.");
            }

            if (continuationToken == null)
                return GetQueryableWithPaginationSettings(iterator, $"{nameof(WithPagination)}/{1}", pageSize);

            return GetQueryableWithPaginationSettings(iterator, continuationToken, pageSize);
        }

        private static FeedIterator<T> GetQueryableWithPaginationSettings<T>(FeedIterator<T> iterator, string continuationInfo, int pageSize)
        { 
//            if (!iterator.GetType().Name.Equals(DocumentQueryTypeName))
//                return iterator;
//
//            var feedOptions = iterator.GetFeedOptionsForQueryable() ?? new QueryRequestOptions();
//            feedOptions.MaxItemCount = pageSize;
//            feedOptions.RequestContinuation = continuationInfo;
            //iterator.SetFeedOptionsForQueryable(feedOptions);
            return iterator;
        }

        internal static QueryRequestOptions GetQueryRequestOptionsForQueryable<T>(this IQueryable<T> queryable)
        {
            if (!queryable.GetType().Name.Equals(CosmosLinqQueryTypeName))
                return null;
            
            return (QueryRequestOptions) InternalTypeCache.Instance.QueryRequestOptionsFieldInfo.GetValue(queryable.Provider);
        }

        internal static void SetQueryRequestOptionsForQueryable<T>(this IQueryable<T> queryable, QueryRequestOptions requestOptions)
        {
            if (!queryable.GetType().Name.Equals(CosmosLinqQueryTypeName))
                return;

            InternalTypeCache.Instance.GetFieldInfoFromCache(queryable.GetType(), "cosmosQueryRequestOptions", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(queryable, requestOptions);
            InternalTypeCache.Instance.QueryRequestOptionsFieldInfo.SetValue(queryable.Provider, requestOptions);
        }
    }
}