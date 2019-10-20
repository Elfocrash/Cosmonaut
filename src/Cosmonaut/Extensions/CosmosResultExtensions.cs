using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Diagnostics;
using Cosmonaut.Internal;
using Cosmonaut.Response;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Cosmonaut.Extensions
{
    public static class CosmosResultExtensions
    {
        public static bool IsSuccess<TEntity>(this Response<TEntity> response)
        {
            return (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;
        }
        
        public static async Task<(List<TEntity> Items, string ContinuationToken)> ToListWithContinuationAsync<TEntity>(
            this IQueryable<TEntity> queryable, int pageSize, CancellationToken cancellationToken = default)
        {
            var queryRequestOptions = queryable.GetQueryRequestOptionsForQueryable();
            queryRequestOptions.MaxItemCount = pageSize;
            queryable.SetQueryRequestOptionsForQueryable(queryRequestOptions);
            return await GetTokenPagedResultsFromQueryToList(queryable, pageSize, cancellationToken);
        }
        
        public static async Task<List<TEntity>> ToListAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return (await GetResultsFromQueryToList(queryable.ToFeedIterator(), cancellationToken)).items;
        }
        
        public static async Task<List<TEntity>> ToListAsync<TEntity>(
            this FeedIterator<TEntity> iterator, 
            CancellationToken cancellationToken = default)
        {
            return (await GetResultsFromQueryToList(iterator, cancellationToken)).items;
        }
        
        public static async Task<int> CountAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return await queryable.InvokeCosmosCallAsync(
                () => CosmosLinqExtensions.CountAsync(queryable, cancellationToken), queryable.ToString(),
                target: GetAltLocationFromQueryable(queryable));
        }

        public static async Task<int> CountAsync<TEntity>(
            this IQueryable<TEntity> queryable,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            var finalQueryable = queryable.Where(predicate);
            return await CountAsync(finalQueryable, cancellationToken);
        }

        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return (await GetSingleOrFirstFromQueryable(queryable, cancellationToken)).FirstOrDefault();
        }

        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(
            this IQueryable<TEntity> queryable,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            var finalQueryable = queryable.Where(predicate);
            return await finalQueryable.FirstOrDefaultAsync(cancellationToken);
        }

        public static async Task<TEntity> FirstAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return (await GetSingleOrFirstFromQueryable(queryable, cancellationToken)).First();
        }

        public static async Task<TEntity> FirstAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            var finalQueryable = queryable.Where(predicate);
            return await finalQueryable.FirstAsync(cancellationToken);
        }

        public static async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return (await GetSingleOrFirstFromQueryable(queryable, cancellationToken)).SingleOrDefault();
        }

        public static async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            this IQueryable<TEntity> queryable,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            var finalQueryable = queryable.Where(predicate);
            return await finalQueryable.SingleOrDefaultAsync(cancellationToken);
        }

        public static async Task<TEntity> SingleAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return (await GetSingleOrFirstFromQueryable(queryable, cancellationToken)).Single();
        }

        public static async Task<TEntity> SingleAsync<TEntity>(
            this IQueryable<TEntity> queryable,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            var finalQueryable = queryable.Where(predicate);
            return await finalQueryable.SingleAsync(cancellationToken);
        }
        
        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(
            this FeedIterator<TEntity> iterator, 
            CancellationToken cancellationToken = default)
        {
            return (await GetResultsFromQueryForSingleOrFirst(iterator, cancellationToken)).FirstOrDefault();
        }

        public static async Task<TEntity> FirstAsync<TEntity>(
            this FeedIterator<TEntity> iterator, 
            CancellationToken cancellationToken = default)
        {
            return (await GetResultsFromQueryForSingleOrFirst(iterator, cancellationToken)).First();
        }

        public static async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            this FeedIterator<TEntity> iterator, 
            CancellationToken cancellationToken = default)
        {
            return (await GetResultsFromQueryForSingleOrFirst(iterator, cancellationToken)).SingleOrDefault();
        }
        
        public static async Task<TEntity> SingleAsync<TEntity>(
            this FeedIterator<TEntity> iterator, 
            CancellationToken cancellationToken = default)
        {
            return (await GetResultsFromQueryForSingleOrFirst(iterator, cancellationToken)).Single();
        }

        public static async Task<TEntity> MaxAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return await queryable.InvokeCosmosCallAsync(() => CosmosLinqExtensions.MaxAsync(queryable, cancellationToken), queryable.ToString(), target: GetAltLocationFromQueryable(queryable));
        }

        public static async Task<TEntity> MinAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return await queryable.InvokeCosmosCallAsync(() => CosmosLinqExtensions.MinAsync(queryable, cancellationToken), queryable.ToString(), target: GetAltLocationFromQueryable(queryable));
        }

        private static async Task<List<T>> GetSingleOrFirstFromQueryable<T>(IQueryable<T> queryable,
            CancellationToken cancellationToken)
        {
            SetFeedOptionsForSingleOperation(ref queryable, out var requestOptions);

            return await GetResultsFromQueryForSingleOrFirst(queryable, cancellationToken);
        }

        private static void SetFeedOptionsForSingleOperation<T>(ref IQueryable<T> queryable, out QueryRequestOptions requestOptions)
        {
            requestOptions = queryable.GetQueryRequestOptionsForQueryable() ?? new QueryRequestOptions();
            requestOptions.MaxItemCount = 1;
            queryable.SetQueryRequestOptionsForQueryable(requestOptions);
        }

        private static async Task<(List<T> items, string continuationToken)> GetResultsFromQueryToList<T>(FeedIterator<T> iterator, CancellationToken cancellationToken)
        {
            var results = new List<T>();
            string lastContinuationToken = null;
            while (iterator.HasMoreResults)
            {
                var items = await iterator.InvokeExecuteNextAsync(() => iterator.ReadNextAsync(cancellationToken),
                    iterator.ToString(), target: GetAltLocationFromIterator(iterator));
                results.AddRange(items);
                lastContinuationToken = items.ContinuationToken;
            }
            return (results, lastContinuationToken);
        }
        
        private static async Task<(List<T> items, string continuationToken)> GetTokenPagedResultsFromQueryToList<T>(IQueryable<T> queryable, int pageSize, CancellationToken cancellationToken)
        {
            var query = queryable.ToFeedIterator();
            var results = new List<T>();
            var nextPageToken = string.Empty;
            while (query.HasMoreResults)
            {
                if (results.Count == pageSize)
                    break;

                var items = await query.InvokeExecuteNextAsync(() => query.ReadNextAsync(cancellationToken),
                    query.ToString(), target: GetAltLocationFromQueryable(queryable));
                nextPageToken = items.ContinuationToken;
                
                foreach (var item in items)
                {
                    results.Add(item);

                    if (results.Count == pageSize)
                        break;
                }
            }
            return (results, nextPageToken);
        }
        
        private static async Task<List<T>> GetResultsFromQueryForSingleOrFirst<T>(IQueryable<T> queryable, CancellationToken cancellationToken)
        {
            var iterator = queryable.ToFeedIterator();
            return await GetResultsFromQueryForSingleOrFirst(iterator, cancellationToken);
        }
        
        private static async Task<List<T>> GetResultsFromQueryForSingleOrFirst<T>(FeedIterator<T> iterator, CancellationToken cancellationToken)
        {
            var results = new List<T>();
            while (iterator.HasMoreResults)
            {
                var items = await iterator.InvokeExecuteNextAsync(() => iterator.ReadNextAsync(cancellationToken),
                    iterator.ToString(), target: GetAltLocationFromIterator(iterator));
                results.AddRange(items);
                if (results.Any())
                    return results;
            }
            return results;
        }

        private static string GetAltLocationFromQueryable<T>(IQueryable<T> queryable)
        {
            if (!CosmosEventSource.EventSource.IsEnabled())
                return null;

            if (!queryable.GetType().Name.Equals("DocumentQuery`1"))
                return null;

            return string.Empty;//TODO InternalTypeCache.Instance.DocumentFeedOrDbLinkFieldInfo?.GetValue(queryable.Provider)?.ToString();
        }
        
        private static string GetAltLocationFromIterator<T>(FeedIterator<T> iterator)
        {
            if (!CosmosEventSource.EventSource.IsEnabled())
                return null;

            if (!iterator.GetType().Name.Equals("DocumentQuery`1"))
                return null;

            return string.Empty;//TODO InternalTypeCache.Instance.DocumentFeedOrDbLinkFieldInfo?.GetValue(queryable.Provider)?.ToString();
        }
    }
}
