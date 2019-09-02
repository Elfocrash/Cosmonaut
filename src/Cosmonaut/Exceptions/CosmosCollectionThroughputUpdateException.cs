using System;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut.Exceptions
{
    public class CosmosCollectionThroughputUpdateException : Exception
    {
        public CosmosCollectionThroughputUpdateException(Container collection) : base($"Failed to update hroughput of collection {collection.Id}")
        {

        }
    }
}