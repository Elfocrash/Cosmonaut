using Microsoft.Azure.Documents;

namespace Cosmonaut
{
    public class CosmosConstants
    {
        public const string CosmosId = "id";
        public const int MinimumCosmosThroughput = 400;
        public const int DefaultMaximumUpscaleThroughput = 10000;
        public const int TooManyRequestsStatusCode = 429;
        public static readonly IndexingPolicy DefaultIndexingPolicy =
            new IndexingPolicy(new RangeIndex(DataType.Number, -1), new RangeIndex(DataType.String, -1), new SpatialIndex(DataType.Point));
        public static readonly UniqueKeyPolicy DefaultUniqueKeyPolicy = new UniqueKeyPolicy();
    }
}