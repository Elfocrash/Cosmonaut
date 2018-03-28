using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public class CosmosResponse
    {
        public bool IsSuccess => ResourceResponse != null && 
            (int)ResourceResponse.StatusCode >= 200 && 
            (int)ResourceResponse.StatusCode <= 299 && 
            CosmosOperationFailure == CosmosOperationFailure.None;

        public CosmosOperationFailure CosmosOperationFailure { get; set; } = CosmosOperationFailure.None;

        public ResourceResponse<Document> ResourceResponse { get; }

        public CosmosResponse(ResourceResponse<Document> resourceResponse)
        {
            ResourceResponse = resourceResponse;
        }

        public CosmosResponse(CosmosOperationFailure failureType)
        {
            CosmosOperationFailure = failureType;
        }
    }
}