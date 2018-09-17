using Cosmonaut.Attributes;

namespace Cosmonaut.Console
{
    public class Book
    {
        [CosmosPartitionKey]
        public string Name { get; set; }

        public string AnotherRandomProp { get; set; }
        
        public string Id { get; set; }
    }
}