using System;

namespace Cosmonaut.Exceptions
{
    public class IllegalCosmosThroughputException : Exception
    {
        public IllegalCosmosThroughputException() : base("CosmosDB throughput cannot be less than 400.")
        {

        }
    }
}