using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public class CosmosResponse
    {
        public bool IsSuccess => (int)ResourceResponse.StatusCode >= 200 && (int)ResourceResponse.StatusCode <= 299;

        public ResourceResponse<Document> ResourceResponse { get; }

        public CosmosResponse(ResourceResponse<Document> resourceResponse)
        {
            ResourceResponse = resourceResponse;
        }
    }
}