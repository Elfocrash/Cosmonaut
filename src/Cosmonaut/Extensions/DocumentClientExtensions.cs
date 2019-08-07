using System.Reflection;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

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
        
        internal static JsonSerializerSettings GetJsonSerializerSettingsFromClient(this IDocumentClient documentClient)
        {
            try
            {
                return (JsonSerializerSettings) typeof(DocumentClient).GetTypeInfo()
                    .GetField("serializerSettings", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(documentClient);
            }
            catch
            {
                return null;
            }
        }
    }
}