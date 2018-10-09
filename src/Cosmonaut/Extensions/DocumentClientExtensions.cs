using Microsoft.Azure.Documents;

namespace Cosmonaut.Extensions
{
    public static class DocumentClientExtensions
    {
        public static void SetupInfiniteRetries(this IDocumentClient documentClient)
        {
            if (documentClient.ConnectionPolicy == null)
                return;
            documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = int.MaxValue;
        }
    }
}