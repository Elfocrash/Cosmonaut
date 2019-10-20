using System;

namespace Cosmonaut.Exceptions
{
    public class MultiplePartitionKeysException : Exception
    {
        public MultiplePartitionKeysException(Type type) : base($"A container cannot have more than one Partition Keys.")
        {

        }
    }
}