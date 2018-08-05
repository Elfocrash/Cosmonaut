using System;
using System.Net;
using Cosmonaut.Response;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Extensions
{
    public static class ExceptionHandlingExtensions
    {
        internal static CosmosResponse<TEntity> DocumentClientExceptionToCosmosResponse<TEntity>(DocumentClientException exception, TEntity entity) where TEntity : class
        {
            switch (exception.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceNotFound);
                case (HttpStatusCode) CosmosConstants.TooManyRequestsStatusCode:
                    return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.RequestRateIsLarge);
                case HttpStatusCode.Conflict:
                    return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceWithIdAlreadyExists);
            }

            throw exception;
        }

        internal static CosmosResponse<TEntity> ToCosmosResponse<TEntity>(this Exception exception) where TEntity : class
        {
            return ToCosmosResponse<TEntity>(exception, null);
        }

        internal static CosmosResponse<TEntity> ToCosmosResponse<TEntity>(this Exception exception, TEntity entity) where TEntity : class
        {
            if (exception is DocumentClientException documentClientException)
                return DocumentClientExceptionToCosmosResponse(documentClientException, entity);

            throw exception;
        }
    }
}