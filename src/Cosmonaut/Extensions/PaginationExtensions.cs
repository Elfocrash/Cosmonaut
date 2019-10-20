using System;
using System.Linq;
using System.Reflection;
using Cosmonaut.Internal;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut.Extensions
{
    public static class PaginationExtensions
    {
        public const string CosmosLinqQueryTypeName = "CosmosLinqQuery`1";

        /// <summary>
        /// Adds pagination for your CosmosDB query
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

            var skip = (pageNumber - 1) * pageSize;
            return queryable.Skip(skip).Take(pageSize);
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