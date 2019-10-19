using System.IO;
using Cosmonaut.Internal;
using Cosmonaut.Storage;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cosmonaut.Extensions
{
    public static class CosmonautHelpers
    {
        public static CosmosDocument ToCosmonautDocument<TEntity>(this TEntity obj, CosmosSerializer serializer) where TEntity : class
        {
            obj.ValidateEntityForCosmosDb();
            var document = serializer.FromStream<dynamic>(serializer.ToStream(obj));
            
            var actualDocument = new CosmosDocument(document) {Id = obj.GetDocumentId()};

            RemoveDuplicateIds(ref actualDocument);
                
            if (typeof(TEntity).UsesSharedCollection())
                actualDocument.SetPropertyValue(nameof(ISharedCosmosEntity.CosmosEntityName), $"{typeof(TEntity).GetSharedCollectionEntityName()}");

            return actualDocument;
        }

        public static Stream ToCosmonautStream<TEntity>(this TEntity obj, CosmosSerializer serializer)
            where TEntity : class
        {
            var pew = ToCosmonautDocument(obj, serializer);//.ToString().Replace("\r\n", "").Replace("\\", "");
            return SimpleStringSerializer.ToStream(pew.ToString().Replace("\r\n", ""));
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