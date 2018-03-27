using System;

namespace Cosmonaut
{
    public class CosmosEntityWithoutIdException<TEntity> : Exception
    {
        public CosmosEntityWithoutIdException(TEntity entity) : base($"Unable to resolve Id for cosmos entity of type {typeof(TEntity).Name}")
        {
            
        }
    }
}