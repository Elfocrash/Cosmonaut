using System;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Extensions
{
    public static class PaginationExtensions
    {
        /// <summary>
        /// Adds pagination for your CosmosDB query. This is an inefficient and expensive form of pagination because it goes
        /// though all the documents to get to the page you want. The usage of WithPagination with the ContinuationToken is recommended. 
        /// Read more at https://github.com/Elfocrash/Cosmonaut
        /// </summary>
        /// <returns>A specific page of the results that your query matches.</returns>
        public static IQueryable<T> WithPagination<T>(this IQueryable<T> queryable, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be a positive number.");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be a positive number.");
            }

            return GetQueryableWithPaginationSettings(queryable, $"{nameof(WithPagination)}/{pageNumber}", pageSize);
        }

        /// <summary>
        /// Adds pagination for your CosmosDB query. This is an efficient and cheap form of pagination because it doesn't go 
        /// though all the documents to get to the page you want. Read more at https://github.com/Elfocrash/Cosmonaut
        /// </summary>
        /// ///
        /// <param name="queryable">The DocumentQueryable for the operation</param>
        /// <param name="continuationToken">When null or empty string, the first page of items will be returned</param>
        /// <param name="pageSize">The size of the page we are expecting</param>
        /// <returns>A specific page of the results that your query matches.</returns>
        public static IQueryable<T> WithPagination<T>(this IQueryable<T> queryable, string continuationToken, int pageSize)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be a positive number.");
            }

            if (continuationToken == null)
                continuationToken = string.Empty;

            return GetQueryableWithPaginationSettings(queryable, continuationToken, pageSize);
        }

        private static IQueryable<T> GetQueryableWithPaginationSettings<T>(IQueryable<T> queryable, string continuationInfo, int pageSize)
        {
            if (!queryable.GetType().Name.Equals("DocumentQuery`1"))
                return queryable;

            var feedOptions = queryable.GetFeedOptionsForQueryable() ?? new FeedOptions();
            feedOptions.MaxItemCount = pageSize;
            feedOptions.RequestContinuation = continuationInfo;
            queryable.SetFeedOptionsForQueryable(feedOptions);
            return queryable;
        }

        internal static FeedOptions GetFeedOptionsForQueryable<T>(this IQueryable<T> queryable)
        {
            if (!queryable.GetType().Name.Equals("DocumentQuery`1"))
                return null;

            return (FeedOptions)queryable.Provider.GetType().GetTypeInfo().GetField("feedOptions", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(queryable.Provider);
        }

        internal static void SetFeedOptionsForQueryable<T>(this IQueryable<T> queryable, FeedOptions feedOptions)
        {
            if (!queryable.GetType().Name.Equals("DocumentQuery`1"))
                return;

            queryable.GetType().GetTypeInfo().GetField("feedOptions", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(queryable, feedOptions);
            queryable.Provider.GetType().GetTypeInfo().GetField("feedOptions", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(queryable.Provider, feedOptions);
        }
    }
}