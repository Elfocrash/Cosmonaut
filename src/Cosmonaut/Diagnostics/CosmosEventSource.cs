using System;
using System.Diagnostics.Tracing;
using System.Threading;

namespace Cosmonaut.Diagnostics
{
    [EventSource(Name = nameof(CosmosEventSource))]
    public class CosmosEventSource : EventSource
    {
        public const int DependencyEventId = 4000;
        public const int DependencyEventErrorId = 4010;

        private static readonly Lazy<CosmosEventSource> Instance =
            new Lazy<CosmosEventSource>(() => new CosmosEventSource(), LazyThreadSafetyMode.ExecutionAndPublication);
        
        public static CosmosEventSource EventSource => Instance.Value;

        private CosmosEventSource()
        {
        }

        [Event(DependencyEventId, Message = "CosmosDB invocation {1}", Level = EventLevel.Informational)]
        public void TrackSuccess(
            string dependencyTypeName,
            string dependencyName,
            string target, 
            string resultCode,
            string data, 
            long startTime, 
            double durationMilliseconds,
            bool isSuccess,
            string properties)
        {
            if (IsEnabled())
                WriteEvent(DependencyEventId, dependencyTypeName, dependencyName, target, resultCode, data, startTime,
                    durationMilliseconds, isSuccess, properties);
        }

        [Event(DependencyEventErrorId, Message = "CosmosDB invocation failure {1}", Level = EventLevel.Error)]
        public void TrackError(
            string dependencyTypeName,
            string dependencyName, 
            string target,
            string resultCode,
            string data,
            long startTime, 
            double durationMilliseconds,
            string errorType, 
            string errorMessage, 
            string stackTrace,
            bool isSuccess,
            string properties)
        {
            if (IsEnabled())
                WriteEvent(DependencyEventErrorId, dependencyTypeName, dependencyName, target, resultCode, data, startTime,
                    durationMilliseconds, errorType, errorMessage, stackTrace, isSuccess, properties);
        }
    }
}