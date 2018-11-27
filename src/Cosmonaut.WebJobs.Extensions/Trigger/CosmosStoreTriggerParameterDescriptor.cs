using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Host.Protocols;

namespace Cosmonaut.WebJobs.Extensions.Trigger
{
    internal class CosmosStoreTriggerParameterDescriptor : TriggerParameterDescriptor
    {
        public string CollectionName { get; set; }

        public override string GetTriggerReason(IDictionary<string, string> arguments)
        {
            return string.Format(CosmosStoreTriggerConstants.TriggerDescription, CollectionName, DateTime.UtcNow.ToString("o"));
        }
    }
}
