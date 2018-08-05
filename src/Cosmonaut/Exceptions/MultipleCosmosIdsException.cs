using System;

namespace Cosmonaut.Exceptions
{
    public class MultipleCosmosIdsException : Exception
    {
        public MultipleCosmosIdsException(string message) : base (message)
        {
            
        }
    }
}