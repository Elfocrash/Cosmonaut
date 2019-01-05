using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Diagnostics;
using Cosmonaut.Internal;
using Cosmonaut.Response;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cosmonaut.Extensions
{
    public static class CosmosResultExtensions
    {
        public static async Task<List<TEntity>> ToListAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return await GetListFromQueryable(queryable, cancellationToken);
        }

        public static async Task<CosmosPagedResults<TEntity>> ToPagedListAsync<TEntity>(
            this IQueryable<TEntity> queryable,
            CancellationToken cancellationToken = default)
        {
            return await GetPagedListFromQueryable(queryable, cancellationToken);
        }

        public static async Task<int> CountAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return await queryable.InvokeCosmosCallAsync(() => DocumentQueryable.CountAsync(queryable, cancellationToken), queryable.ToString(), target: GetAltLocationFromQueryable(queryable));
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

        public static async Task<TEntity> MaxAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return await queryable.InvokeCosmosCallAsync(() => DocumentQueryable.MaxAsync(queryable, cancellationToken), queryable.ToString(), target: GetAltLocationFromQueryable(queryable));
        }

        public static async Task<TEntity> MinAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default)
        {
            return await queryable.InvokeCosmosCallAsync(() => DocumentQueryable.MinAsync(queryable, cancellationToken), queryable.ToString(), target: GetAltLocationFromQueryable(queryable));
        }

        private static async Task<List<T>> GetListFromQueryable<T>(IQueryable<T> queryable,
            CancellationToken cancellationToken)
        {
            var feedOptions = queryable.GetFeedOptionsForQueryable();
            if (feedOptions?.RequestContinuation == null)
            {
                return await GetResultsFromQueryToList(queryable, cancellationToken);
            }

            return await GetPaginatedResultsFromQueryable(queryable, cancellationToken, feedOptions);
        }

        private static async Task<List<T>> GetSingleOrFirstFromQueryable<T>(IQueryable<T> queryable,
            CancellationToken cancellationToken)
        {
            SetFeedOptionsForSingleOperation(ref queryable, out var feedOptions);

            if (feedOptions?.RequestContinuation == null)
            {
                return await GetResultsFromQueryForSingleOrFirst(queryable, cancellationToken);
            }

            return await GetPaginatedResultsFromQueryable(queryable, cancellationToken, feedOptions);
        }

        private static void SetFeedOptionsForSingleOperation<T>(ref IQueryable<T> queryable, out FeedOptions feedOptions)
        {
            feedOptions = queryable.GetFeedOptionsForQueryable() ?? new FeedOptions();
            feedOptions.MaxItemCount = 1;
            queryable.SetFeedOptionsForQueryable(feedOptions);
        }

        private static async Task<CosmosPagedResults<T>> GetPagedListFromQueryable<T>(IQueryable<T> queryable,
            CancellationToken cancellationToken)
        {
            var feedOptions = queryable.GetFeedOptionsForQueryable();
            if (feedOptions?.RequestContinuation == null)
                return new CosmosPagedResults<T>(await GetListFromQueryable(queryable, cancellationToken), feedOptions?.MaxItemCount ?? 0,
                    string.Empty, queryable);

            return await GetPaginatedResultsFromQueryable(queryable, cancellationToken, feedOptions);
        }

        private static async Task<List<T>> GetResultsFromQueryToList<T>(IQueryable<T> queryable, CancellationToken cancellationToken)
        {
            var query = queryable.AsDocumentQuery();
            var results = new List<T>();
            while (query.HasMoreResults)
            {
                var items = await query.InvokeExecuteNextAsync(() => query.ExecuteNextAsync<T>(cancellationToken),
                    query.ToString(), target: GetAltLocationFromQueryable(queryable));
                results.AddRange(items);
            }
            return results;
        }

        private static async Task<List<T>> GetResultsFromQueryForSingleOrFirst<T>(IQueryable<T> queryable, CancellationToken cancellationToken)
        {
            var query = queryable.AsDocumentQuery();
            var results = new List<T>();
            while (query.HasMoreResults)
            {
                var items = await query.InvokeExecuteNextAsync(() => query.ExecuteNextAsync<T>(cancellationToken),
                    query.ToString(), target: GetAltLocationFromQueryable(queryable));
                results.AddRange(items);
                if (results.Any())
                    return results;
            }
            return results;
        }

        private static async Task<CosmosPagedResults<T>> GetSkipTakePagedResultsFromQueryToList<T>(IQueryable<T> queryable, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            var query = queryable.AsDocumentQuery();
            var results = new List<T>();
            var documentsSkipped = 0;
            var nextPageToken = string.Empty;
            while (query.HasMoreResults)
            {
                if (results.Count == pageSize)
                    break;

                var items = await query.InvokeExecuteNextAsync(() => query.ExecuteNextAsync<T>(cancellationToken),
                    query.ToString(), target: GetAltLocationFromQueryable(queryable));
                nextPageToken = items.ResponseContinuation;
                
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
            return new CosmosPagedResults<T>(results, pageSize, nextPageToken, queryable);
        }

        private static async Task<CosmosPagedResults<T>> GetTokenPagedResultsFromQueryToList<T>(IQueryable<T> queryable, int pageSize, CancellationToken cancellationToken)
        {
            var query = queryable.AsDocumentQuery();
            var results = new List<T>();
            var nextPageToken = string.Empty;
            while (query.HasMoreResults)
            {
                if (results.Count == pageSize)
                    break;

                var items = await query.InvokeExecuteNextAsync(() => query.ExecuteNextAsync<T>(cancellationToken),
                    query.ToString(), target: GetAltLocationFromQueryable(queryable));
                nextPageToken = items.ResponseContinuation;
                
                foreach (var item in items)
                {
                    results.Add(item);

                    if (results.Count == pageSize)
                        break;
                }
            }
            return new CosmosPagedResults<T>(results, pageSize, nextPageToken, queryable);
        }

        private static async Task<CosmosPagedResults<T>> GetPaginatedResultsFromQueryable<T>(IQueryable<T> queryable, CancellationToken cancellationToken,
            FeedOptions feedOptions)
        {
            var usesSkipTakePagination =
                feedOptions.RequestContinuation.StartsWith(nameof(PaginationExtensions.WithPagination));

            if (!usesSkipTakePagination)
                return await GetTokenPagedResultsFromQueryToList(queryable, feedOptions.MaxItemCount ?? 0,
                    cancellationToken);

            var pageNumber = int.Parse(feedOptions.RequestContinuation.Replace(
                $"{nameof(PaginationExtensions.WithPagination)}/", string.Empty));
            feedOptions.RequestContinuation = null;
            queryable.SetFeedOptionsForQueryable(feedOptions);
            return await GetSkipTakePagedResultsFromQueryToList(queryable, pageNumber, feedOptions.MaxItemCount ?? 0,
                cancellationToken);
        }

        private static string GetAltLocationFromQueryable(IQueryable queryable)
        {
            if (!CosmosEventSource.EventSource.IsEnabled())
                return null;

            if (!queryable.GetType().Name.Equals("DocumentQuery`1"))
                return null;

            return InternalTypeCache.Instance.DocumentFeedOrDbLinkFieldInfo?.GetValue(queryable.Provider)?.ToString();
        }
    }
}