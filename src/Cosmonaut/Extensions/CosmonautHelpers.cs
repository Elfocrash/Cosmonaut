using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cosmonaut.Extensions
{
    public static class CosmonautHelpers
    {
        public static Document ToCosmonautDocument<TEntity>(this TEntity obj, JsonSerializerSettings settings) where TEntity : class
        {
            obj.ValidateEntityForCosmosDb();
            var document = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(obj, settings), settings);

            using (JsonReader reader = new JTokenReader(document))
            {
                var actualDocument = new Document();
                actualDocument.LoadFrom(reader);
                actualDocument.Id = obj.GetDocumentId();
                RemoveDuplicateIds(ref actualDocument);
                
                if (typeof(TEntity).UsesSharedCollection())
                    actualDocument.SetPropertyValue(nameof(ISharedCosmosEntity.CosmosEntityName), $"{typeof(TEntity).GetSharedCollectionEntityName()}");

                return actualDocument;
            }
        }

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
        
        internal static void RemoveDuplicateIds(ref Document actualDocument)
        {
            actualDocument.SetPropertyValue("Id", null);
            actualDocument.SetPropertyValue("ID", null);
            actualDocument.SetPropertyValue("iD", null);
        }
    }
}