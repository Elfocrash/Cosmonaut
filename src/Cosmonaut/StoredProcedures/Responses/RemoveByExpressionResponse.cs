using Newtonsoft.Json;

namespace Cosmonaut.StoredProcedures.Responses
{
    public class RemoveByExpressionResponse
    {
        [JsonProperty("removedCount")]
        public int RemovedCount { get; set; }

        [JsonProperty("success")]
        public bool IsSuccess { get; set; }
    }
}