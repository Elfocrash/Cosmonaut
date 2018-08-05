using Cosmonaut.Attributes;

namespace Cosmonaut.Console
{
    [SharedCosmosCollection("shared", "books")]
    public class Book : ISharedCosmosEntity
    {
        public string Name { get; set; }

        public string AnotherRandomProp { get; set; }

        public string Id { get; set; }

        public string CosmosEntityName { get; set; }
    }
}