using System.Linq;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Extensions
{
    public static class CollectionExtensions
    {
        public static string GetCollectionPartitionKeyName(this DocumentCollection collection)
        {
            return collection?.PartitionKey?.Paths?.FirstOrDefault()?.Trim('/') ?? string.Empty;
        }
    }
}