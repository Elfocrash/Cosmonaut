using System;

namespace Cosmonaut.Exceptions
{
    public class IllegalCosmosThroughputException : Exception
    {
        public IllegalCosmosThroughputException() : base("CosmosDB throughput must be between 400 and 10000")
        {

        }
    }
}