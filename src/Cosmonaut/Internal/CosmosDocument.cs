using Newtonsoft.Json.Linq;

namespace Cosmonaut.Internal
{
    public class CosmosDocument : CosmosResource
    {
        public CosmosDocument(JObject json) : base(json)
        {
        }
    }
}