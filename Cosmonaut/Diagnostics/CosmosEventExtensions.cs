using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Threading.Tasks;

namespace Cosmonaut.Diagnostics
{
    public static class CosmosEventExtensions
    {
        public static Task<TResult> InvokeCosmosCallAsync<TResult>(
            this object invoker,
            Func<Task<TResult>> eventCall,
            string data = null,
            string target = null,
            string name = null)
        {
            return CreateCosmosEventCall(invoker, eventCall, data, target, name).InvokeAsync();
        }

        public static Task InvokeCosmosCallAsync(
            this object invoker,
            Func<Task> dependencyCall,
            string data = null,
            string target = null,
            string name = null)
        {
            return CreateCosmosEventCall(invoker, async () =>
            {
                await dependencyCall();
                return true;
            }, data, target, name).InvokeAsync();
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

        internal static CosmosEventCall<TResult> CreateCosmosEventCall<TResult>(
            this object agent,
            Func<Task<TResult>> dependencyCall,
            string data = null,
            string target = null,
            string name = null)
        {
            var dependencyData = new CosmosEventMetadata()
            {
                DependencyTypeName = GetAgentName(agent),
                DependencyName = name ?? dependencyCall.GetMethodInfo().Name,
                Target = target,
                Data = data
            };

            return new CosmosEventCall<TResult>(dependencyCall, dependencyData);
        }
    }
}