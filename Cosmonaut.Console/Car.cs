using Cosmonaut.Attributes;

namespace Cosmonaut.Console
{
    [SharedCosmosCollection("shared", "cars")]
    public class Car
    {
        public string Id { get; set; }
        
        public string ModelName { get; set; }
    }
}