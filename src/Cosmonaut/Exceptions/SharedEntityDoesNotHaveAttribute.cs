using System;
using Cosmonaut.Attributes;

namespace Cosmonaut.Exceptions
{
    public class SharedEntityDoesNotHaveAttribute : Exception
    {
        public SharedEntityDoesNotHaveAttribute(Type type) : base($"Shared entity {type.Name} implements {nameof(ISharedCosmosEntity)} but must also have the {nameof(SharedCosmosCollectionAttribute)}")
        {

        }
    }
}