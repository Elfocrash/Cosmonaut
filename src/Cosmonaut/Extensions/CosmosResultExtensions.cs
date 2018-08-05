using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Diagnostics;
using Microsoft.Azure.Documents.Linq;

namespace Cosmonaut.Extensions
{
    public static class CosmosResultExtensions
    {
        public static async Task<List<TEntity>> ToListAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default) where TEntity : class
        {
            return await GetListFromQueryable(queryable, cancellationToken);
        }

        public static async Task<int> CountAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default) where TEntity : class
        {
            return await queryable.InvokeCosmosCallAsync(() => DocumentQueryable.CountAsync(queryable, cancellationToken), queryable.ToString());
        }

        public static async Task<int> CountAsync<TEntity>(
            this IQueryable<TEntity> queryable,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var finalQueryable = queryable.Where(predicate);
            return await CountAsync(finalQueryable, cancellationToken);
        }

        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default) where TEntity : class
        {
            return (await GetListFromQueryable(queryable, cancellationToken)).FirstOrDefault();
        }

        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(
            this IQueryable<TEntity> queryable,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var finalQueryable = queryable.Where(predicate);
            return await finalQueryable.FirstOrDefaultAsync(cancellationToken);
        }

        public static async Task<TEntity> FirstAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default) where TEntity : class
        {
            return (await GetListFromQueryable(queryable, cancellationToken)).First();
        }

        public static async Task<TEntity> FirstAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var finalQueryable = queryable.Where(predicate);
            return await finalQueryable.FirstAsync(cancellationToken);
        }

        public static async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default) where TEntity : class
        {
            return (await GetListFromQueryable(queryable, cancellationToken)).SingleOrDefault();
        }

        public static async Task<TEntity> SingleOrDefaultAsync<TEntity>(
            this IQueryable<TEntity> queryable,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var finalQueryable = queryable.Where(predicate);
            return await finalQueryable.SingleOrDefaultAsync(cancellationToken);
        }

        public static async Task<TEntity> SingleAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default) where TEntity : class
        {
            return (await GetListFromQueryable(queryable, cancellationToken)).Single();
        }

        public static async Task<TEntity> SingleAsync<TEntity>(
            this IQueryable<TEntity> queryable,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var finalQueryable = queryable.Where(predicate);
            return await finalQueryable.SingleAsync(cancellationToken);
        }

        public static async Task<TEntity> MaxAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default) where TEntity : class
        {
            return await queryable.InvokeCosmosCallAsync(() => DocumentQueryable.MaxAsync(queryable, cancellationToken), queryable.ToString());
        }

        public static async Task<TEntity> MinAsync<TEntity>(
            this IQueryable<TEntity> queryable, 
            CancellationToken cancellationToken = default) where TEntity : class
        {
            return await queryable.InvokeCosmosCallAsync(() => DocumentQueryable.MinAsync(queryable, cancellationToken), queryable.ToString());
        }

        internal static async Task<T> SingleOrDefaultGenericAsync<T>(
            this IQueryable<T> queryable,
            CancellationToken cancellationToken = default)
        {
            return (await GetListFromQueryable(queryable, cancellationToken)).SingleOrDefault();
        }

        internal static async Task<List<T>> ToGenericListAsync<T>(
            this IQueryable<T> queryable,
            CancellationToken cancellationToken = default)
        {
            return await GetListFromQueryable(queryable, cancellationToken);
        }

        private static async Task<List<T>> GetListFromQueryable<T>(IQueryable<T> queryable,
            CancellationToken cancellationToken)
        {
            var query = queryable.AsDocumentQuery();
            var results = await GetResultsFromQueryToList(query, cancellationToken);
            return results;
        }

        private static async Task<List<T>> GetResultsFromQueryToList<T>(IDocumentQuery<T> query, CancellationToken cancellationToken)
        {
            var results = new List<T>();
            while (query.HasMoreResults)
            {
                var items = await query.InvokeExecuteNextAsync(() => query.ExecuteNextAsync<T>(cancellationToken), query.ToString()).ExecuteCosmosCommand();
                results.AddRange(items);
            }
            return results;
        }
    }
}