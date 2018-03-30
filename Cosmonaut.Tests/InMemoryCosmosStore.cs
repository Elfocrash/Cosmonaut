using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cosmonaut.Response;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Tests
{
    public class InMemoryCosmosStore<TEntity> : ICosmosStore<TEntity> where TEntity : class
    {
        private readonly ConcurrentDictionary<string, TEntity> _store;
        private readonly CosmosDocumentProcessor<TEntity> _documentProcessor;

        public InMemoryCosmosStore()
        {
            _store = new ConcurrentDictionary<string, TEntity>();
            _documentProcessor = new CosmosDocumentProcessor<TEntity>();
        }

        public async Task<CosmosResponse<TEntity>> AddAsync(TEntity entity)
        {
            _documentProcessor.ValidateEntityForCosmosDb(entity);
            var id = _documentProcessor.GetDocumentId(entity);
            if(_store.TryAdd(id, entity))
                return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.Success);
            return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceWithIdAlreadyExists);
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
            _documentProcessor.ValidateEntityForCosmosDb(entity);
            var id = _documentProcessor.GetDocumentId(entity);
            var exists = _store[id];
            if (exists == null)
                return new CosmosResponse<TEntity>(CosmosOperationStatus.ResourceNotFound);

            _store[id] = entity;

            return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.Success);
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

        public async Task<CosmosResponse<TEntity>> RemoveAsync(TEntity entity)
        {
            _documentProcessor.ValidateEntityForCosmosDb(entity);
            var id = _documentProcessor.GetDocumentId(entity);
            if(_store.TryRemove(id, out var outEntity))
                return new CosmosResponse<TEntity>(outEntity, CosmosOperationStatus.Success);

            return new CosmosResponse<TEntity>(CosmosOperationStatus.ResourceNotFound);
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
            if (_store.TryRemove(id, out var outEntity))
                return new CosmosResponse<TEntity>(outEntity, CosmosOperationStatus.Success);

            return new CosmosResponse<TEntity>(CosmosOperationStatus.ResourceNotFound);
        }
        
        public async Task<CosmosMultipleResponse<TEntity>> RemoveAsync(Func<TEntity, bool> predicate)
        {
            var toRemove = _store.Values.Where(predicate).ToList();
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

        public async Task<List<TEntity>> ToListAsync(Func<TEntity, bool> predicate = null)
        {
            if (predicate == null)
                predicate = entity => true;

            return _store.Values.Where(predicate).ToList();
        }

        public Task<IOrderedQueryable<TEntity>> QueryableAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> FirstOrDefaultAsync(Func<TEntity, bool> predicate)
        {
            throw new NotImplementedException();
        }
    }
}