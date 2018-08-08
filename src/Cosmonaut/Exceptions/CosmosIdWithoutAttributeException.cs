using System;

namespace Cosmonaut.Exceptions
{
    public class CosmosIdWithoutAttributeException<TEntity> : Exception
    {
        public CosmosIdWithoutAttributeException() : 
            base($"Entity {typeof(TEntity).Name} doesn't have the Id property decorated with [JsonProperty(\"id\")] attribute. " +
                 $"Please mark your entity's id property with the [JsonProperty(\"id\")] attribute.")
        {
            
        }
    }
}