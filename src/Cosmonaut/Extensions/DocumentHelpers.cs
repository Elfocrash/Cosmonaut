using Microsoft.Azure.Documents;

namespace Cosmonaut.Extensions
{
    internal class DocumentHelpers
    {
        internal static PartitionKeyDefinition GetPartitionKeyDefinition(string partitionKeyName)
        {
            return new PartitionKeyDefinition
            {
                Paths =
                {
                    $"/{partitionKeyName}"
                }
            };
        }
    }
}