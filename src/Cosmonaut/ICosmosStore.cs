using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Exceptions;
using Cosmonaut.Response;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public interface ICosmosStore<TEntity> where TEntity : class
    {
        /// <summary>
        ///     Entry point to the usage of LINQ in order to query the collection. It is highly recommended to get the results with the .ToListAsync method
        ///     because it is using the internal paginated retrieval to prevent locking.
        /// </summary>
        /// <param name="feedOptions">The feed options for this operation.</param>
        IQueryable<TEntity> Query(FeedOptions feedOptions = null);

        /// <summary>
        ///     Returns an IQueryable that matches the expression provided. You can use ToListAsync to enumerate it or add WithPagination for
        ///     pagination support.
        /// </summary>
        /// <param name="sql">The sql query for this operation.</param>
        /// <param name="parameters">The sql parameters to replace if any</param>
        /// <param name="feedOptions">The feed options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        IQueryable<TEntity> Query(string sql, object parameters = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns a single item that matches the expression provided.
        /// </summary>
        /// <param name="sql">The sql query for this operation.</param>
        /// <param name="parameters">The sql parameters to replace if any</param>
        /// <param name="feedOptions">The feed options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        Task<TEntity> QuerySingleAsync(string sql, object parameters = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns a single item of any type that matches the expression provided.
        /// </summary>
        /// <param name="sql">The sql query for this operation.</param>
        /// <param name="parameters">The sql parameters to replace if any</param>
        /// <param name="feedOptions">The feed options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        Task<T> QuerySingleAsync<T>(string sql, object parameters = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns a collection of items that match the expression provided.
        /// </summary>
        /// <param name="sql">The sql query for this operation.</param>
        /// <param name="parameters">The sql parameters to replace if any</param>
        /// <param name="feedOptions">The feed options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        Task<IEnumerable<TEntity>> QueryMultipleAsync(string sql, object parameters = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns a collection of items of any type that match the expression provided.
        /// </summary>
        /// <param name="sql">The sql query for this operation.</param>
        /// <param name="parameters">The sql parameters to replace if any</param>
        /// <param name="feedOptions">The feed options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        Task<IEnumerable<T>> QueryMultipleAsync<T>(string sql, object parameters = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Adds the given entity in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to add.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        /// <returns> 
        ///     A task that represents the asynchronous Add operation. The task result contains the
        ///     <see cref="CosmosResponse{TEntity}"/> for the entity. The response provides access to 
        ///     various response information such as whether it was successful or what (if anything) went wrong.
        /// </returns>
        /// <exception cref="MultipleCosmosIdsException">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity has more that one Ids specified for it.
        /// </exception>
        /// <exception cref="CosmosEntityWithoutIdException{TEntity}">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity does not have an Id specified.
        /// </exception>
        Task<CosmosResponse<TEntity>> AddAsync(TEntity entity, RequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        ///     Adds the given entities in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to add.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        /// <returns> 
        ///     A task that represents the asynchronous AddRange operation. The task result contains the
        ///     <see cref="CosmosMultipleResponse{TEntity}"/> for the entities. The response provides access to 
        ///     various response information such as whether it was successful or what (if anything) went wrong
        ///     at the individual entity level.
        /// </returns>
        /// <exception cref="MultipleCosmosIdsException">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity has more that one Ids specified for it.
        /// </exception>
        /// <exception cref="CosmosEntityWithoutIdException{TEntity}">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity does not have an Id specified.
        /// </exception>
        Task<CosmosMultipleResponse<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, Func<TEntity, RequestOptions> requestOptions = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        ///     Updates the given entity in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to update.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        /// <returns> 
        ///     A task that represents the asynchronous Update operation. The task result contains the
        ///     <see cref="CosmosResponse{TEntity}"/> for the entity. The response provides access to 
        ///     various response information such as whether it was successful or what (if anything) went wrong.
        /// </returns>
        /// <exception cref="MultipleCosmosIdsException">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity has more that one Ids specified for it.
        /// </exception>
        /// <exception cref="CosmosEntityWithoutIdException{TEntity}">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity does not have an Id specified.
        /// </exception>
        Task<CosmosResponse<TEntity>> UpdateAsync(TEntity entity, RequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        ///     Updates the given entities in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to update.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        /// <returns> 
        ///     A task that represents the asynchronous Update operation. The task result contains the
        ///     <see cref="CosmosResponse{TEntity}"/> for the entity. The response provides access to 
        ///     various response information such as whether it was successful or what (if anything) went wrong
        ///     at the individual entity level.
        /// </returns>
        /// <exception cref="MultipleCosmosIdsException">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity has more that one Ids specified for it.
        /// </exception>
        /// <exception cref="CosmosEntityWithoutIdException{TEntity}">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity does not have an Id specified.
        /// </exception>
        Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, Func<TEntity, RequestOptions> requestOptions = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        ///     Adds if absent or updates if present the given entity in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to upsert.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        /// <returns> 
        ///     A task that represents the asynchronous Upsert operation. The task result contains the
        ///     <see cref="CosmosResponse{TEntity}"/> for the entity. The response provides access to 
        ///     various response information such as whether it was successful or what (if anything) went wrong.
        /// </returns>
        /// <exception cref="MultipleCosmosIdsException">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity has more that one Ids specified for it.
        /// </exception>
        /// <exception cref="CosmosEntityWithoutIdException{TEntity}">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity does not have an Id specified.
        /// </exception>
        Task<CosmosResponse<TEntity>> UpsertAsync(TEntity entity, RequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        ///     Adds if absent or updates if present the given entities in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to upsert.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        /// <returns> 
        ///     A task that represents the asynchronous Upsert operation. The task result contains the
        ///     <see cref="CosmosResponse{TEntity}"/> for the entity. The response provides access to 
        ///     various response information such as whether it was successful or what (if anything) went wrong
        ///     at the individual entity level.
        /// </returns>
        /// <exception cref="MultipleCosmosIdsException">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity has more that one Ids specified for it.
        /// </exception>
        /// <exception cref="CosmosEntityWithoutIdException{TEntity}">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity does not have an Id specified.
        /// </exception>
        //Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(IEnumerable<TEntity> entities, RequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(IEnumerable<TEntity> entities, Func<TEntity, RequestOptions> requestOptions = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        ///     Removed all the entities matching the given criteria.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="predicate">The entities to remove.</param>
        /// <param name="feedOptions">The feed options for this operation.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
        /// <param name="cancellationToken">The cancellation token for this operation.</param>
        /// <returns> 
        ///     A task that represents the asynchronous Remove operation. The task result contains the
        ///     <see cref="CosmosMultipleResponse{TEntity}"/> for the entities. The response provides access to 
        ///     various response information such as whether it was successful or what (if anything) went wrong
        ///     at the individual entity level.
        /// </returns>
        Task<CosmosMultipleResponse<TEntity>> RemoveAsync(Expression<Func<TEntity, bool>> predicate, FeedOptions feedOptions = null, Func<TEntity, RequestOptions> requestOptions = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        ///     Removes the given entity from the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to remove.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        /// <returns> 
        ///     A task that represents the asynchronous Remove operation. The task result contains the
        ///     <see cref="CosmosResponse{TEntity}"/> for the entity. The response provides access to 
        ///     various response information such as whether it was successful or what (if anything) went wrong 
        ///      at the individual entity level.
        /// </returns>
        /// <exception cref="CosmosEntityWithoutIdException{TEntity}">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity does not have an Id specified.
        /// </exception>
        /// <exception cref="MultipleCosmosIdsException">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity has more that one Ids specified for it.
        /// </exception>
        Task<CosmosResponse<TEntity>> RemoveAsync(TEntity entity, RequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        ///     Removes the given entities from the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to remove.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        /// <returns> 
        ///     A task that represents the asynchronous RemoveRange operation. The task result contains the
        ///     <see cref="CosmosMultipleResponse{TEntity}"/> for the entities. The response provides access to 
        ///     various response information such as whether it was successful or what (if anything) went wrong
        ///      at the individual entity level.
        /// </returns>
        /// <exception cref="CosmosEntityWithoutIdException{TEntity}">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity does not have an Id specified.
        /// </exception>
        /// <exception cref="MultipleCosmosIdsException">
        ///     An error is encountered while processing the entity.
        ///     This is because the given entity has more that one Ids specified for it.
        /// </exception>
        Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(IEnumerable<TEntity> entities, Func<TEntity, RequestOptions> requestOptions = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        ///     Removes the entity with he specified Id from the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The id of the entity attempting to remove. </param>
        /// <param name="requestOptions">The request options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        /// <returns> 
        ///     A task that represents the asynchronous RemoveById operation. The task result contains the
        ///     <see cref="CosmosResponse{TEntity}"/> for the entity. The response provides access to 
        ///     various response information such as whether it was successful or what (if anything) went wrong.
        /// </returns>
        Task<CosmosResponse<TEntity>> RemoveByIdAsync(string id, RequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns an entity by document/entity id from the cosmos db store. If the collection is partitioned you will need to provide the
        ///     partition key value in the <see cref="RequestOptions"/>.
        /// </summary>
        /// <param name="id">The id of the document/entity.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        /// <returns>The entity that matches the id and partition key. Returns null if the entity is not found.</returns>
        Task<TEntity> FindAsync(string id, RequestOptions requestOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns an entity by document/entity id and partition key value from the cosmos db store.
        /// </summary>
        /// <param name="id">The id of the document/entity.</param>
        /// <param name="partitionKeyValue">The partition key value.</param>
        /// <param name="cancellationToken">The CancellationToken for this operation.</param>
        /// <returns>The entity that matches the id and partition key. Returns null if the entity is not found.</returns>
        Task<TEntity> FindAsync(string id, object partitionKeyValue, CancellationToken cancellationToken = default);

        /// <summary>
        ///     The settings that were used to initialise this CosmosStore
        /// </summary>
        CosmosStoreSettings Settings { get; }

        /// <summary>
        ///     Indicates whether this is a shared CosmosStore
        /// </summary>
        bool IsShared { get; }

        /// <summary>
        ///     The name of the collection that this CosmosStore is targeting
        /// </summary>
        string CollectionName { get; }

        /// <summary>
        ///     The name of the database that this CosmosStore is targeting
        /// </summary>
        string DatabaseName { get; }

        ICosmonautClient CosmonautClient { get; }
    }
}