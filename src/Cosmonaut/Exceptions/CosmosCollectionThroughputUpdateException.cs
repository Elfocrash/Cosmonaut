using System;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Exceptions
{
    public class CosmosCollectionThroughputUpdateException : Exception
    {
        public CosmosCollectionThroughputUpdateException(DocumentCollection collection) : base($"Failed to update hroughput of collection {collection.Id}")
        {

        }
    }
}