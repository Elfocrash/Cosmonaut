using Cosmonaut.Attributes;
using Newtonsoft.Json;

namespace Cosmonaut.Unit
{
    public class Dummy
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    [CosmosCollection("dummies")]
    public class DummyWithThroughput
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    public class DummyImplEntity : CosmosEntity
    {
        public string Name { get; set; }
    }

    public class DummyImplEntityWithAttr : CosmosEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string Name { get; set; }
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

    public class DummyNoId
    {
        public string Name { get; set; }
    }

    [SharedCosmosCollection("shared", "dummies")]
    public class DummySharedCollection : ISharedCosmosEntity
    {
        public string CosmosEntityName { get; set; }
    }

    [SharedCosmosCollection("")]
    public class DummySharedCollectionEmpty
    {

    }

    public class DummyImplNoAttribute : ISharedCosmosEntity
    {
        public string CosmosEntityName { get; set; }
    }

    [SharedCosmosCollection("shared")]
    public class DummyWithAttributeNoImpl
    {

    }
}