using System;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut.Exceptions
{
    public class CosmosContainerThroughputUpdateException : Exception
    {
        public CosmosContainerThroughputUpdateException(Container container) : base($"Failed to update throughput of container {container.Id}")
        {

        }
    }
}