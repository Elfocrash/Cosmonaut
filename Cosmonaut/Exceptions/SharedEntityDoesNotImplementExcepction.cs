using System;

namespace Cosmonaut.Exceptions
{
    public class SharedEntityDoesNotImplementExcepction : Exception
    {
        public SharedEntityDoesNotImplementExcepction(Type type) : base($"Shared entity {type.Name} has appropriate attribute but must also implement {nameof(ISharedCosmosEntity)}")
        {

        }
    }
}