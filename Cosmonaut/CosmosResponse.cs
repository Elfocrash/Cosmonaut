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
            CosmosOperationStatus == CosmosOperationStatus.Success;

        public CosmosOperationStatus CosmosOperationStatus { get; set; } = CosmosOperationStatus.Success;

        public ResourceResponse<Document> ResourceResponse { get; }

        public CosmosResponse(ResourceResponse<Document> resourceResponse)
        {
            ResourceResponse = resourceResponse;
        }

        public CosmosResponse(CosmosOperationStatus statusType)
        {
            CosmosOperationStatus = statusType;
        }
    }
}