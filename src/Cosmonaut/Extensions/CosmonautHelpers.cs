using Cosmonaut.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cosmonaut.Extensions
{
    public static class CosmonautHelpers
    {
        public static CosmosDocument ToCosmonautDocument<TEntity>(this TEntity obj, JsonSerializerSettings settings) where TEntity : class
        {
            obj.ValidateEntityForCosmosDb();
            var document = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(obj, settings), settings);

//            var actualDocument = new CosmosDocument(document) {Id = obj.GetDocumentId()};
//
//            RemoveDuplicateIds(ref actualDocument);
//                
//            if (typeof(TEntity).UsesSharedCollection())
//                actualDocument.SetPropertyValue(nameof(ISharedCosmosEntity.CosmosEntityName), $"{typeof(TEntity).GetSharedCollectionEntityName()}");
//
//            return actualDocument;
            return new CosmosDocument(null);
        }

        internal static string GetPartitionKeyDefinition(string partitionKeyName)
        {
            return $"/{partitionKeyName}";
        }
        
        internal static void RemoveDuplicateIds(ref CosmosDocument actualDocument)
        {
            actualDocument.SetPropertyValue("Id", null);
            actualDocument.SetPropertyValue("ID", null);
            actualDocument.SetPropertyValue("iD", null);
        }
    }
}