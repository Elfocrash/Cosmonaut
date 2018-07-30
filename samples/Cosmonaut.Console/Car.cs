using Cosmonaut.Attributes;

namespace Cosmonaut.Console
{
    [SharedCosmosCollection("shared", "cars")]
    public class Car : ISharedCosmosEntity
    {
        public string Id { get; set; }
        
        public string ModelName { get; set; }

        public string CosmosEntityName { get; set; }
    }
}