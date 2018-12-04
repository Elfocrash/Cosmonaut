namespace Cosmonaut.WebJobs.Extensions.Trigger
{
    internal static class CosmosStoreTriggerConstants
    {
        public const string DefaultLeaseCollectionName = "leases";

        public const string TriggerName = "CosmosStoreTrigger";

        public const string TriggerDescription = "New changes on collection {0} at {1}";

        public const string InvokeString = "{0} changes detected.";
    }
}