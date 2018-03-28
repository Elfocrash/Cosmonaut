using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cosmonaut.Response;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public interface ICosmosStore<TEntity> where TEntity : class
    {
        Task<CosmosResponse> AddAsync(TEntity entity, RequestOptions requestOptions = null);

        Task<IEnumerable<CosmosResponse>> AddRangeAsync(params TEntity[] entities);

        Task<IEnumerable<CosmosResponse>> AddRangeAsync(IEnumerable<TEntity> entities);

        Task<TEntity> FirstOrDefaultAsync(Func<TEntity, bool> predicate);

        Task RemoveAsync(Func<TEntity, bool> predicate);

        Task<CosmosResponse> RemoveAsync(TEntity entity);

        Task<CosmosResponse> UpdateAsync(TEntity entity);

        Task<CosmosResponse> RemoveByIdAsync(string id);

        Task<List<TEntity>> ToListAsync(Func<TEntity, bool> predicate = null);

        Task<IOrderedQueryable<TEntity>> QueryableAsync();

        Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate);
    }
}