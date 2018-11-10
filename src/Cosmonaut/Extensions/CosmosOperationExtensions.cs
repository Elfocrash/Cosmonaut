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

        internal static async Task<TResult> ExecuteCosmosQuery<TResult>(this Task<DocumentResponse<TResult>> operationTask) where TResult : class
        {
            try
            {
                var response = await operationTask;
                return response?.Document;
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

        internal static async Task<ResourceResponse<TResult>> ExecuteCosmosCommand<TResult>(this Task<ResourceResponse<TResult>> operationTask) where TResult : Resource, new()
        {
            try
            {
                var response = await operationTask;
                return response;
            }
            catch (DocumentClientException exception)
            {
                var cosmosReponse = exception.ToCosmosResponse<TResult>();

                if (cosmosReponse.CosmosOperationStatus == CosmosOperationStatus.ResourceNotFound) return null;

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
                var cosmosReponse = exception.ToCosmosResponse(entity);

                if (cosmosReponse.CosmosOperationStatus == CosmosOperationStatus.ResourceNotFound) return cosmosReponse;

                throw;
            }
        }
    }
}