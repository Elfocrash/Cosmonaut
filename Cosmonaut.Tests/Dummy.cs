using Newtonsoft.Json;

namespace Cosmonaut.Tests
{
    public class Dummy
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    public class DummyImplEntity : ICosmosEntity
    {
        public string Name { get; set; }

        public string CosmosId { get; set; }
    }

    public class DummyImplEntityWithAttr : ICosmosEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string Name { get; set; }

        public string CosmosId { get; set; }
    }

    public class DummyWithIdAndWithAttr
    {
        [JsonProperty("id")]
        public string ActualyId { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }
    }

    public class DummyWithMultipleAttr
    {
        [JsonProperty("id")]
        public string ActualyId { get; set; }

        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class DummyWithIdAttrOnId
    {
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}