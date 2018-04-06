using System;

namespace Cosmonaut.Exceptions
{
    public class MultiplePartitionKeysException : Exception
    {
        public MultiplePartitionKeysException(Type type) : base($"A collection cannot have more than one Partition Keys.")
        {

        }
    }
}