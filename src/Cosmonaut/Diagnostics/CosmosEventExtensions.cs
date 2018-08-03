using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Diagnostics
{
    public static class CosmosEventExtensions
    {
        public static Task<TResult> InvokeCosmosCallAsync<TResult>(
            this object invoker,
            Func<Task<TResult>> eventCall,
            string data,
            Dictionary<string, object> properties = null,
            string target = null,
            string name = null)
        {
            return CreateCosmosEventCall(invoker, data, properties, target, name).InvokeAsync(eventCall);
        }

        public static Task<ResourceResponse<TResource>> InvokeCosmosOperationAsync<TResource>(
            this object invoker,
            Func<Task<ResourceResponse<TResource>>> eventCall,
            string data,
            Dictionary<string, object> properties = null,
            string target = null,
            string name = null) where TResource : Resource, new()
        {
            return CreateCosmosEventCall(invoker, data, properties, target, name).InvokeAsync(eventCall);
        }

        public static Task<FeedResponse<TEntity>> InvokeExecuteNextAsync<TEntity>(
            this object invoker,
            Func<Task<FeedResponse<TEntity>>> eventCall,
            string data,
            Dictionary<string, object> properties = null,
            string target = null,
            string name = null)
        {
            return CreateCosmosEventCall(invoker, data, properties, target, name).InvokeAsync(eventCall);
        }

        public static bool IsDependencyTrackingEventSource(this EventSource eventSource)
        {
            return string.Equals(eventSource.Name, EventSource.GetName(typeof(CosmosEventSource)));
        }

        public static CosmosEventMetadata AsDependency(this IDictionary<string, object> eventData)
        {
            return CosmosEventDataConverter.ConvertToDependencyFromEventData(eventData);
        }

        internal static string GetAgentName(this object invoker)
        {
            var type = invoker.GetType();

            if (!type.IsConstructedGenericType)
            {
                return type.Name;
            }

            var i = type.Name.IndexOf('`');

            return i <= -1 ? type.Name.Substring(0, i) : type.Name;
        }

        internal static CosmosEventCall CreateCosmosEventCall(
            this object agent,
            string data,
            Dictionary<string, object> properties = null,
            string target = null,
            string name = null)
        {
            var dependencyData = new CosmosEventMetadata()
            {
                DependencyTypeName = GetAgentName(agent),
                Target = target,
                DependencyName = name,
                Data = data,
                Properties = properties ?? new Dictionary<string, object>()
            };

            return new CosmosEventCall(dependencyData);
        }
    }
}