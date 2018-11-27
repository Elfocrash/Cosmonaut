using System.Collections.Generic;
using Cosmonaut.Shared;
using Cosmonaut.WebJobs.Extensions.Trigger;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Cosmonaut.AzureFunction
{
    public static class CosmosStoreTriggerFunction
    {
        [FunctionName("CosmosStoreTriggerFunction")]
        public static void Run([CosmosStoreTrigger(
            "localtest",
            typeof(Entity),
            ServiceEndpoint = "CosmosEndpoint",
            AuthKey = "CosmosAuthKey",
            LeaseConnectionStringSetting = "LeaseSettings",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Entity> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
            }
        }
    }
}
