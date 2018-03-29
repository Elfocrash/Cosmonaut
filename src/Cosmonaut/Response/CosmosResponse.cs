using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Response
{
    public class CosmosResponse<TEntity> where TEntity : class
    {
        public bool IsSuccess => ResourceResponse != null && 
            (int)ResourceResponse.StatusCode >= 200 && 
            (int)ResourceResponse.StatusCode <= 299 && 
            CosmosOperationStatus == CosmosOperationStatus.Success;

        public CosmosOperationStatus CosmosOperationStatus { get; set; } = CosmosOperationStatus.Success;

        public ResourceResponse<Document> ResourceResponse { get; }

        public TEntity Entity { get; set; }

        public CosmosResponse(ResourceResponse<Document> resourceResponse)
        {
            ResourceResponse = resourceResponse;
        }
        public CosmosResponse(CosmosOperationStatus statusType)
        {
            CosmosOperationStatus = statusType;
        }

        public CosmosResponse(TEntity entity, ResourceResponse<Document> resourceResponse)
        {
            ResourceResponse = resourceResponse;
            Entity = entity;
        }

        public CosmosResponse(TEntity entity, CosmosOperationStatus statusType)
        {
            CosmosOperationStatus = statusType;
            Entity = entity;
        }
    }
}