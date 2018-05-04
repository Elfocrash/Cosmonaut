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
        ///     Exposes the lower level DocumentClient to the consumer.
        /// </summary>
        IDocumentClient DocumentClient { get; }
    }
}