using System;

namespace Cosmonaut.Exceptions
{
    public class SharedContainerNameMissingException : Exception
    {
        public SharedContainerNameMissingException(Type type) : base($"Unable to resolve shared container name for type {type.Name}")
        {

        }
    }
}