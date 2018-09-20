using Cosmonaut.Attributes;

namespace Cosmonaut.System.Models
{
    [SharedCosmosCollection("onlycoolanimals")]
    public class Alpaca : Animal, ISharedCosmosEntity
    {
        public string Id { get; set; }

        public string CosmosEntityName { get; set; }
    }
}