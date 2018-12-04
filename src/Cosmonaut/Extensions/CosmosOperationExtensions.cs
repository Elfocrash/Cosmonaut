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
                var cosmosResponse = exception.ToCosmosResponse<TResult>();

                if (cosmosResponse.CosmosOperationStatus == CosmosOperationStatus.ResourceNotFound)
                    return null;

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
                var cosmosResponse = exception.ToCosmosResponse<TResult>();

                if (cosmosResponse.CosmosOperationStatus == CosmosOperationStatus.ResourceNotFound)
                    return null;

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
                var cosmosResponse = exception.ToCosmosResponse<TResult>();

                if (cosmosResponse.CosmosOperationStatus == CosmosOperationStatus.ResourceNotFound)
                    return null;

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
                var cosmosResponse = exception.ToCosmosResponse(entity);

                switch (cosmosResponse.CosmosOperationStatus)
                {
                    case CosmosOperationStatus.ResourceNotFound:
                    case CosmosOperationStatus.PreconditionFailed:
                    case CosmosOperationStatus.Conflict:
                        return cosmosResponse;
                    default:
                        throw;
                }
            }
        }
    }
}