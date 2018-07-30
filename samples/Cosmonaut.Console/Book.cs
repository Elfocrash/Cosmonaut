using Cosmonaut.Attributes;
using Newtonsoft.Json;

namespace Cosmonaut.Console
{
    [CosmosCollection("booktest")]
    public class Book
    {
        public string Name { get; set; }

        public string AnotherRandomProp { get; set; }
        
        public string Id { get; set; }
    }
}