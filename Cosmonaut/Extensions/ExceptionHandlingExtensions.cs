using System;
using Cosmonaut.Response;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Extensions
{
    public static class ExceptionHandlingExtensions
    {
        internal static CosmosResponse<TEntity> HandleDocumentClientException<TEntity>(DocumentClientException exception, TEntity entity) where TEntity : class
        {
            if (exception.Message.Contains(CosmosConstants.ResourceNotFoundMessage))
                return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceNotFound);

            if (exception.Message.Contains(CosmosConstants.RequestRateIsLargeMessage))
                return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.RequestRateIsLarge);

            if (exception.Message.Contains(CosmosConstants.ResourceWithIdExistsMessage))
                return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceWithIdAlreadyExists);

            throw exception;
        }

        internal static CosmosResponse<TEntity> HandleOperationException<TEntity>(this Exception exception) where TEntity : class
        {
            return HandleOperationException<TEntity>(exception, null);
        }

        internal static CosmosResponse<TEntity> HandleOperationException<TEntity>(this Exception exception, TEntity entity) where TEntity : class
        {
            if (exception is DocumentClientException documentClientException)
                return HandleDocumentClientException(documentClientException, entity);

            return new CosmosResponse<TEntity>(entity, exception);
        }
    }
}