using Cosmonaut.Attributes;
using Newtonsoft.Json;

namespace Cosmonaut.Console
{
    [SharedCosmosCollection("shared", "cars")]
    public class Car : CosmosEntity, ISharedCosmosEntity
    {        
        public string ModelName { get; set; }

        public string CosmosEntityName { get; set; }
    }
}