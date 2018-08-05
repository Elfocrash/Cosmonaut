using System;

namespace Cosmonaut.Exceptions
{
    public class SharedCollectionNameMissingException : Exception
    {
        public SharedCollectionNameMissingException(Type type) : base($"Unable to resolve shared collection name for type {type.Name}")
        {

        }
    }
}