using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Cosmonaut.Response
{
    public class CosmosResponse<TEntity> where TEntity : class
    {
        public bool IsSuccess => CosmosOperationStatus == CosmosOperationStatus.Success;

        public CosmosOperationStatus CosmosOperationStatus { get; } = CosmosOperationStatus.Success;

        public ResourceResponse<Document> ResourceResponse { get; }

        public TEntity Entity { get; }

        public Exception Exception { get; }

        internal CosmosResponse(ResourceResponse<Document> resourceResponse)
        {
            ResourceResponse = resourceResponse;
        }

        internal CosmosResponse(TEntity entity, ResourceResponse<Document> resourceResponse)
        {
            ResourceResponse = resourceResponse;
            Entity = entity;
        }
        
        internal CosmosResponse(TEntity entity, Exception exception, CosmosOperationStatus statusType)
        {
            CosmosOperationStatus = statusType;
            Entity = entity;
            Exception = exception;
        }

        public static implicit operator TEntity(CosmosResponse<TEntity> response)
        {
            if (response?.Entity != null)
                return response.Entity;

            if (!string.IsNullOrEmpty(response?.ResourceResponse?.Resource?.ToString()))
                return JsonConvert.DeserializeObject<TEntity>(response.ResourceResponse.Resource.ToString());

            return null;
        }
    }
}