using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Response;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;

namespace Cosmonaut.Tests
{
    public class InMemoryCosmosStore<TEntity> : ICosmosStore<TEntity> where TEntity : class
    {
        private readonly ConcurrentDictionary<string, TEntity> _store;

        public InMemoryCosmosStore()
        {
            _store = new ConcurrentDictionary<string, TEntity>();
        }

        public async Task<CosmosResponse<TEntity>> AddAsync(TEntity entity)
        {
            return await Task.Run(() =>
                {
                    entity.ValidateEntityForCosmosDb();
                    var id = entity.GetDocumentId();
                    if (_store.TryAdd(id, entity))
                        return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.Success);
                    return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceWithIdAlreadyExists);
                }
            );
        }

        public async Task<CosmosMultipleResponse<TEntity>> AddRangeAsync(params TEntity[] entities)
        {
            return await AddRangeAsync((IEnumerable<TEntity>) entities);
        }

        public async Task<CosmosMultipleResponse<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            var response = new CosmosMultipleResponse<TEntity>();
            foreach (var entity in entities)
            {
                var result = await AddAsync(entity);
                if(!result.IsSuccess)
                    response.FailedEntities.Add(result);
            }
            return response;
        }

        public async Task<CosmosResponse<TEntity>> UpdateAsync(TEntity entity)
        {
            return await Task.Run(() =>
            {
                entity.ValidateEntityForCosmosDb();
                var id = entity.GetDocumentId();

                if (!_store.ContainsKey(id))
                    return new CosmosResponse<TEntity>(CosmosOperationStatus.ResourceNotFound);

                _store[id] = entity;
                return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.Success);
            });
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(params TEntity[] entities)
        {
            return await UpdateRangeAsync((IEnumerable<TEntity>)entities);
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            var response = new CosmosMultipleResponse<TEntity>();
            foreach (var entity in entities)
            {
                var result = await UpdateAsync(entity);
                if (!result.IsSuccess)
                    response.FailedEntities.Add(result);
            }
            return response;
        }

        public Task<CosmosResponse<TEntity>> UpsertAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(params TEntity[] entities)
        {
            throw new NotImplementedException();
        }

        public Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public async Task<CosmosResponse<TEntity>> RemoveAsync(TEntity entity)
        {
            return await Task.Run(()=>
            {
                entity.ValidateEntityForCosmosDb();
                var id = entity.GetDocumentId();
                if (_store.TryRemove(id, out var outEntity))
                {
                    return new CosmosResponse<TEntity>(outEntity, CosmosOperationStatus.Success);
                }

                return new CosmosResponse<TEntity>(CosmosOperationStatus.ResourceNotFound);
            });
        }

        public async Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(params TEntity[] entities)
        {
            return await RemoveRangeAsync((IEnumerable<TEntity>)entities);
        }

        public async Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            var response = new CosmosMultipleResponse<TEntity>();
            foreach (var entity in entities)
            {
                var result = await RemoveAsync(entity);
                if (!result.IsSuccess)
                    response.FailedEntities.Add(result);
            }
            return response;
        }

        public async Task<CosmosResponse<TEntity>> RemoveByIdAsync(string id)
        {
            return await Task.Run(() =>
            {
                if (_store.TryRemove(id, out var outEntity))
                    return new CosmosResponse<TEntity>(outEntity, CosmosOperationStatus.Success);

                return new CosmosResponse<TEntity>(CosmosOperationStatus.ResourceNotFound);
            });
        }

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            throw new NotImplementedException();
        }

        public async Task<CosmosMultipleResponse<TEntity>> RemoveAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var toRemove = _store.Values.Where(predicate.Compile()).ToList();
            var response = new CosmosMultipleResponse<TEntity>();
            foreach (var entity in toRemove)
            {
                var result = await RemoveAsync(entity);
                if (!result.IsSuccess)
                    response.FailedEntities.Add(result);
            }
            return response;
        }

        public IDocumentClient DocumentClient { get; }

        public Task<IDocumentQuery<TEntity>> AsDocumentQueryAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            return await Task.Run(() =>
            {

                if (predicate == null)
                    predicate = entity => true;

                return _store.Values.Where(predicate.Compile()).ToList();
            });
        }

        public Task<IOrderedQueryable<TEntity>> QueryableAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return (IQueryable<TEntity>) await Task.Run(() =>
            {
                var pred = predicate.Compile();
                return _store.Values.Where(pred);
            });
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Task.Run(() => _store.Values.FirstOrDefault(predicate.Compile()));
        }
    }
}