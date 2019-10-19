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
        
        public static async Task<List<TEntity>> ToListAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return await GetResultsFromQueryToList(queryable.ToFeedIterator(), cancellationToken);
        }
        
        public static async Task<List<TEntity>> ToListAsync<TEntity>(
            this FeedIterator<TEntity> iterator, 
            CancellationToken cancellationToken = default)
        {
            return await GetResultsFromQueryToList(iterator, cancellationToken);
        }

//        public static async Task<CosmosPagedResults<TEntity>> ToPagedListAsync<TEntity>(
//            this IQueryable<TEntity> queryable,
//            CancellationToken cancellationToken = default)
//        {
//            return await GetPagedListFromQueryable(queryable, cancellationToken);
//        }

        /*public static async Task<int> CountAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return await queryable.InvokeCosmosCallAsync(() => DocumentQueryable.CountAsync(queryable, cancellationToken), queryable.ToString(), target: GetAltLocationFromQueryable(queryable));
        }*/

//        public static async Task<int> CountAsync<TEntity>(
//            this IQueryable<TEntity> queryable,
//            Expression<Func<TEntity, bool>> predicate,
//            CancellationToken cancellationToken = default)
//        {
//            var finalQueryable = queryable.Where(predicate);
//            return await CountAsync(finalQueryable, cancellationToken);
//        }

//        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(
//            this IQueryable<TEntity> queryable, 
//            CancellationToken cancellationToken = default)
//        {
//            return (await GetSingleOrFirstFromQueryable(queryable, cancellationToken)).FirstOrDefault();
//        }

//        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(
//            this IQueryable<TEntity> queryable,
//            Expression<Func<TEntity, bool>> predicate,
//            CancellationToken cancellationToken = default)
//        {
//            var finalQueryable = queryable.Where(predicate);
//            return await finalQueryable.FirstOrDefaultAsync(cancellationToken);
//        }
//
//        public static async Task<TEntity> FirstAsync<TEntity>(
//            this IQueryable<TEntity> queryable, 
//            CancellationToken cancellationToken = default)
//        {
//            return (await GetSingleOrFirstFromQueryable(queryable, cancellationToken)).First();
//        }
//
//        public static async Task<TEntity> FirstAsync<TEntity>(
//            this IQueryable<TEntity> queryable, 
//            Expression<Func<TEntity, bool>> predicate,
//            CancellationToken cancellationToken = default)
//        {
//            var finalQueryable = queryable.Where(predicate);
//            return await finalQueryable.FirstAsync(cancellationToken);
//        }
//
//        public static async Task<TEntity> SingleOrDefaultAsync<TEntity>(
//            this IQueryable<TEntity> queryable, 
//            CancellationToken cancellationToken = default)
//        {
//            return (await GetSingleOrFirstFromQueryable(queryable, cancellationToken)).SingleOrDefault();
//        }
//
//        public static async Task<TEntity> SingleOrDefaultAsync<TEntity>(
//            this IQueryable<TEntity> queryable,
//            Expression<Func<TEntity, bool>> predicate,
//            CancellationToken cancellationToken = default)
//        {
//            var finalQueryable = queryable.Where(predicate);
//            return await finalQueryable.SingleOrDefaultAsync(cancellationToken);
//        }
//
//        public static async Task<TEntity> SingleAsync<TEntity>(
//            this IQueryable<TEntity> queryable, 
//            CancellationToken cancellationToken = default)
//        {
//            return (await GetSingleOrFirstFromQueryable(queryable, cancellationToken)).Single();
//        }
//
//        public static async Task<TEntity> SingleAsync<TEntity>(
//            this IQueryable<TEntity> queryable,
//            Expression<Func<TEntity, bool>> predicate,
//            CancellationToken cancellationToken = default)
//        {
//            var finalQueryable = queryable.Where(predicate);
//            return await finalQueryable.SingleAsync(cancellationToken);
//        }

//        public static async Task<TEntity> MaxAsync<TEntity>(
//            this IQueryable<TEntity> queryable, 
//            CancellationToken cancellationToken = default)
//        {
//            Microsoft.Azure.Cosmos.Linq.CosmosLinqExtensions.ToQueryDefinition()
//            return await queryable.InvokeCosmosCallAsync(() => DocumentQueryable.MaxAsync(queryable, cancellationToken), queryable.ToString(), target: GetAltLocationFromQueryable(queryable));
//        }
//
//        public static async Task<TEntity> MinAsync<TEntity>(
//            this IQueryable<TEntity> queryable, 
//            CancellationToken cancellationToken = default)
//        {
//            return await queryable.InvokeCosmosCallAsync(() => DocumentQueryable.MinAsync(queryable, cancellationToken), queryable.ToString(), target: GetAltLocationFromQueryable(queryable));
//        }

//        private static async Task<List<T>> GetListFromQueryable<T>(FeedIterator<T> iterator,
//            CancellationToken cancellationToken)
//        {
//            var feedOptions = iterator.GetFeedOptionsForQueryable();
//            if (feedOptions?.RequestContinuation == null)
//            {
//                return await GetResultsFromQueryToList(iterator, cancellationToken);
//            }
//
//            return await GetPaginatedResultsFromQueryable(iterator, cancellationToken, feedOptions);
//        }
//
//        private static async Task<List<T>> GetSingleOrFirstFromQueryable<T>(IQueryable<T> queryable,
//            CancellationToken cancellationToken)
//        {
//            SetFeedOptionsForSingleOperation(ref queryable, out var feedOptions);
//
//            if (feedOptions?.RequestContinuation == null)
//            {
//                return await GetResultsFromQueryForSingleOrFirst(queryable, cancellationToken);
//            }
//
//            return await GetPaginatedResultsFromQueryable(queryable, cancellationToken, feedOptions);
//        }
//
//        private static void SetFeedOptionsForSingleOperation<T>(ref IQueryable<T> queryable, out FeedOptions feedOptions)
//        {
//            feedOptions = queryable.GetFeedOptionsForQueryable() ?? new FeedOptions();
//            feedOptions.MaxItemCount = 1;
//            queryable.SetFeedOptionsForQueryable(feedOptions);
//        }

//        private static async Task<CosmosPagedResults<T>> GetPagedListFromQueryable<T>(IQueryable<T> queryable,
//            CancellationToken cancellationToken)
//        {
//            var feedOptions = queryable.GetFeedOptionsForQueryable();
//            if (feedOptions?.RequestContinuation == null)
//                return new CosmosPagedResults<T>(await GetListFromQueryable(queryable, cancellationToken), feedOptions?.MaxItemCount ?? 0,
//                    string.Empty, queryable);
//
//            return await GetPaginatedResultsFromQueryable(queryable, cancellationToken, feedOptions);
//        }

        private static async Task<List<T>> GetResultsFromQueryToList<T>(FeedIterator<T> iterator, CancellationToken cancellationToken)
        {
            var results = new List<T>();
            while (iterator.HasMoreResults)
            {
                var items = await iterator.InvokeExecuteNextAsync(() => iterator.ReadNextAsync(cancellationToken),
                    iterator.ToString(), target: string.Empty /*target: GetAltLocationFromQueryable(queryable)*/);
                results.AddRange(items);
            }
            return results;
        }

        private static async Task<List<T>> GetResultsFromQueryForSingleOrFirst<T>(FeedIterator<T> iterator, CancellationToken cancellationToken)
        {
            var results = new List<T>();
            while (iterator.HasMoreResults)
            {
                var items = await iterator.InvokeExecuteNextAsync(() => iterator.ReadNextAsync(cancellationToken),
                    iterator.ToString(), target: GetAltLocationFromQueryable(iterator));
                results.AddRange(items);
                if (results.Any())
                    return results;
            }
            return results;
        }

        private static async Task<CosmosPagedResults<T>> GetSkipTakePagedResultsFromQueryToList<T>(FeedIterator<T> iterator, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            var results = new List<T>();
            var documentsSkipped = 0;
            var nextPageToken = string.Empty;
            while (iterator.HasMoreResults)
            {
                if (results.Count == pageSize)
                    break;

                var items = await iterator.InvokeExecuteNextAsync(() => iterator.ReadNextAsync(cancellationToken),
                    iterator.ToString(), target: GetAltLocationFromQueryable(iterator));
                nextPageToken = items.ContinuationToken;
                
                foreach (var item in items)
                {
                    if (documentsSkipped < ((pageNumber - 1) * pageSize))
                    {
                        documentsSkipped++;
                        continue;
                    }

                    results.Add(item);

                    if (results.Count == pageSize)
                        break;
                }
            }
            return new CosmosPagedResults<T>(results, pageSize, nextPageToken, iterator);
        }

        private static async Task<CosmosPagedResults<T>> GetTokenPagedResultsFromQueryToList<T>(FeedIterator<T> iterator, int pageSize, CancellationToken cancellationToken)
        {
            var results = new List<T>();
            var nextPageToken = string.Empty;
            while (iterator.HasMoreResults)
            {
                if (results.Count == pageSize)
                    break;

                var items = await iterator.InvokeExecuteNextAsync(() => iterator.ReadNextAsync(cancellationToken),
                    iterator.ToString(), target: GetAltLocationFromQueryable(iterator));
                nextPageToken = items.ContinuationToken;
                
                foreach (var item in items)
                {
                    results.Add(item);

                    if (results.Count == pageSize)
                        break;
                }
            }
            return new CosmosPagedResults<T>(results, pageSize, nextPageToken, iterator);
        }

//        private static async Task<CosmosPagedResults<T>> GetPaginatedResultsFromQueryable<T>(IQueryable<T> queryable, CancellationToken cancellationToken,
//            FeedOptions feedOptions)
//        {
//            var usesSkipTakePagination =
//                feedOptions.RequestContinuationToken.StartsWith(nameof(PaginationExtensions.WithPagination));
//
//            if (!usesSkipTakePagination)
//                return await GetTokenPagedResultsFromQueryToList(queryable, feedOptions.MaxItemCount ?? 0,
//                    cancellationToken);
//
//            var pageNumber = int.Parse(feedOptions.RequestContinuationToken.Replace(
//                $"{nameof(PaginationExtensions.WithPagination)}/", string.Empty));
//            feedOptions.RequestContinuationToken = null;
//            queryable.SetFeedOptionsForQueryable(feedOptions);
//            return await GetSkipTakePagedResultsFromQueryToList(queryable, pageNumber, feedOptions.MaxItemCount ?? 0,
//                cancellationToken);
//        }

        private static string GetAltLocationFromQueryable<T>(FeedIterator<T> queryable)
        {
            if (!CosmosEventSource.EventSource.IsEnabled())
                return null;

            if (!queryable.GetType().Name.Equals("DocumentQuery`1"))
                return null;

            return string.Empty;//InternalTypeCache.Instance.DocumentFeedOrDbLinkFieldInfo?.GetValue(queryable.Provider)?.ToString();
        }
    }
}