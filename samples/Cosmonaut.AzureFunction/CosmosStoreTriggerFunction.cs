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
        public static void Run(
            [CosmosStoreTrigger(
                "localtest",
                ConnectionStringSetting = "CosmosConnectionString",
                LeaseConnectionStringSetting = "LeaseSettings",
                LeaseCollectionName = "leases",
                CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Llama> llamaInput,
            ILogger log)
        {
            if (llamaInput != null && llamaInput.Count > 0)
            {
                log.LogInformation("Llamas modified " + llamaInput.Count);
                log.LogInformation("First document Id " + llamaInput[0].Id);
            }
        }
    }
}
