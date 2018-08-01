using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Response;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Extensions
{
    internal static class CosmosOperationExtensions
    {
        internal static async Task<TResult> ExecuteCosmosQuery<TResult>(this Task<ResourceResponse<TResult>> operationTask) where TResult : Resource, new()
        {
            try
            {
                var response = await operationTask;
                return response?.Resource;
            }
            catch (DocumentClientException exception)
            {
                var cosmosReponse = exception.ToCosmosResponse<TResult>();

                switch (cosmosReponse.CosmosOperationStatus)
                {
                    case CosmosOperationStatus.ResourceNotFound:
                        return null;
                    case CosmosOperationStatus.RequestRateIsLarge:
                        await Task.Delay(exception.RetryAfter);
                        return await ExecuteCosmosQuery(operationTask);
                }
                throw;
            }
        }

        internal static async Task<CosmosResponse<TEntity>> ExecuteCosmosCommand<TEntity>(this Task<ResourceResponse<Document>> operationTask, TEntity entity) where TEntity : class
        {
            try
            {
                var response = await operationTask;
                return new CosmosResponse<TEntity>(entity, response);
            }
            catch (DocumentClientException exception)
            {
                var cosmosReponse = exception.ToCosmosResponse<TEntity>();

                if (cosmosReponse.CosmosOperationStatus != CosmosOperationStatus.RequestRateIsLarge)
                    return cosmosReponse;

                await Task.Delay(exception.RetryAfter);
                return await ExecuteCosmosCommand(operationTask, entity);
            }
        }

        internal static async Task<CosmosResponse<TEntity>> ExecuteCosmosCommand<TEntity>(this Task<ResourceResponse<Document>> operationTask) where TEntity : class
        {
            try
            {
                var response = await operationTask;
                return new CosmosResponse<TEntity>(response);
            }
            catch (DocumentClientException exception)
            {
                var cosmosReponse = exception.ToCosmosResponse<TEntity>();

                if (cosmosReponse.CosmosOperationStatus != CosmosOperationStatus.RequestRateIsLarge)
                    return cosmosReponse;

                await Task.Delay(exception.RetryAfter);
                return await ExecuteCosmosCommand<TEntity>(operationTask);
            }
        }
        
        internal static async Task<FeedResponse<T>> ExecuteCosmosCommand<T>(this Task<FeedResponse<T>> operationTask)
        {
            try
            {
                return await operationTask;
            }
            catch (DocumentClientException exception)
            {
                if (exception.StatusCode != (HttpStatusCode?) 429) throw;

                await Task.Delay(exception.RetryAfter);
                return await ExecuteCosmosCommand(operationTask);

            }
        }
    }
}