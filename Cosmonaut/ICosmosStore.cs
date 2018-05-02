using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Exceptions;
using Cosmonaut.Response;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cosmonaut
{
    public interface ICosmosStore<TEntity> where TEntity : class
    {
        /// <summary>
        ///     Adds the given entity in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to add.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
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
        Task<CosmosResponse<TEntity>> AddAsync(TEntity entity, RequestOptions requestOptions = null);


        /// <summary>
        ///     Adds the given entities in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to add.</param>
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
        Task<CosmosMultipleResponse<TEntity>> AddRangeAsync(params TEntity[] entities);


        /// <summary>
        ///     Adds the given entities in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to add.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
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
        Task<CosmosMultipleResponse<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, RequestOptions requestOptions = null);


        /// <summary>
        ///     Updates the given entity in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to update.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
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
        Task<CosmosResponse<TEntity>> UpdateAsync(TEntity entity, RequestOptions requestOptions = null);


        /// <summary>
        ///     Updates the given entities in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to update.</param>
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
        Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(params TEntity[] entities);


        /// <summary>
        ///     Updates the given entities in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to update.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
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
        Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, RequestOptions requestOptions = null);


        /// <summary>
        ///     Adds if absent or updates if present the given entity in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to upsert.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
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
        Task<CosmosResponse<TEntity>> UpsertAsync(TEntity entity, RequestOptions requestOptions = null);


        /// <summary>
        ///     Adds if absent or updates if present the given entities in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to upsert.</param>
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
        Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(params TEntity[] entities);


        /// <summary>
        ///     Adds if absent or updates if present the given entities in the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to upsert.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
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
        Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(IEnumerable<TEntity> entities, RequestOptions requestOptions = null);


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
        Task<CosmosMultipleResponse<TEntity>> RemoveAsync(Expression<Func<TEntity, bool>> predicate, FeedOptions feedOptions = null, RequestOptions requestOptions = null, CancellationToken cancellationToken = default);


        /// <summary>
        ///     Removes the given entity from the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to remove.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
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
        Task<CosmosResponse<TEntity>> RemoveAsync(TEntity entity, RequestOptions requestOptions = null);


        /// <summary>
        ///     Removes the given entities from the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to remove.</param>
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
        Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(params TEntity[] entities);


        /// <summary>
        ///     Removes the given entities from the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to remove.</param>
        /// <param name="requestOptions">The request options for this operation.</param>
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
        Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(IEnumerable<TEntity> entities, RequestOptions requestOptions = null);


        /// <summary>
        ///     Removes the entity with he specified Id from the cosmos db store.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The id of the entity attempting to remove. </param>
        /// <param name="requestOptions">The request options for this operation.</param>
        /// <returns> 
        ///     A task that represents the asynchronous RemoveById operation. The task result contains the
        ///     <see cref="CosmosResponse{TEntity}"/> for the entity. The response provides access to 
        ///     various response information such as whether it was successful or what (if anything) went wrong.
        /// </returns>
        Task<CosmosResponse<TEntity>> RemoveByIdAsync(string id, RequestOptions requestOptions = null);

        /// <summary>
        ///     Returns the count of documents that much the query in the cosmos db store.
        /// </summary>
        /// <param name="predicate">The expression that the query is based on. </param>
        /// <param name="feedOptions">The feed options for this operation.</param>
        /// <param name="cancellationToken">The cancellation token for this operation.</param>
        /// <returns> 
        ///     A task that represents the asynchronous Count operation. The task result contains the
        ///     count of the collection.
        /// </returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Exposes the lower level DocumentClient to the consumer.
        /// </summary>
        IDocumentClient DocumentClient { get; }

        Task<IDocumentQuery<TEntity>> AsDocumentQueryAsync(Expression<Func<TEntity, bool>> predicate = null, FeedOptions feedOptions = null);

        Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns only specific columns of documents based on a query.
        ///     In order to add the Id column in whatever you are selecting you MUST meet one of the following:
        ///     Either decorate your id property with the [JsonProperty("id")] attribute or extend the CosmosEntity class.
        /// </summary>
        /// <typeparam name="TResult">The type of object that will be returned from this operation.</typeparam>
        /// <param name="selector">The selector expression for the fields to be returned.</param>
        /// <param name="predicate">The filter expression in order to filter down the results.</param>
        /// <param name="feedOptions">The feed options for the operation.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns></returns>
        Task<List<TResult>> SelectToListAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        [Obsolete("Use ToListAsync() instead. This will be dropped.")]
        Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate, FeedOptions feedOptions = null);

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, FeedOptions feedOptions = null , CancellationToken cancellationToken = default);
    }
}