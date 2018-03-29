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
        Task<CosmosResponse<TEntity>> AddAsync(TEntity entity, RequestOptions requestOptions = null);

        Task<CosmosMultipleResponse<TEntity>> AddRangeAsync(params TEntity[] entities);

        Task<CosmosMultipleResponse<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);

        Task<TEntity> FirstOrDefaultAsync(Func<TEntity, bool> predicate);

        Task<CosmosMultipleResponse<TEntity>> RemoveAsync(Func<TEntity, bool> predicate);

        Task<CosmosResponse<TEntity>> RemoveAsync(TEntity entity);

        Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(params TEntity[] entities);

        Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(IEnumerable<TEntity> entities);

        Task<CosmosResponse<TEntity>> UpdateAsync(TEntity entity);

        Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(params TEntity[] entities);

        Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities);

        Task<CosmosResponse<TEntity>> RemoveByIdAsync(string id);

        Task<List<TEntity>> ToListAsync(Func<TEntity, bool> predicate = null);

        Task<IOrderedQueryable<TEntity>> QueryableAsync();

        Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate);
    }
}