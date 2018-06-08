using System;
using System.Diagnostics.Tracing;

namespace Cosmonaut.Diagnostics
{
    [EventSource(Name = nameof(CosmosEventSource))]
    public class CosmosEventSource : EventSource
    {
        public const int DependencyEventId = 4000;
        public const int DependencyEventErrorId = 4010;

        private static readonly Lazy<CosmosEventSource> Instance =
            new Lazy<CosmosEventSource>(() => new CosmosEventSource());
        
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
            int managedThreadId, 
            bool isSuccess = true)
        {
            if (IsEnabled())
                WriteEvent(DependencyEventId, dependencyTypeName, dependencyName, target, resultCode, data, startTime,
                    durationMilliseconds, managedThreadId, isSuccess);
        }

        [Event(DependencyEventErrorId, Message = "CosmosDB invocation failure {1}", Level = EventLevel.Error)]
        public void TrackError(
            string dependencyTypeName,
            string dependencyName, 
            string target, 
            string data,
            long startTime, 
            double durationMilliseconds,
            string errorType, 
            string errorMessage, 
            string stackTrace,
            int managedThreadId, 
            bool isSuccess = false)
        {
            if (IsEnabled())
                WriteEvent(DependencyEventErrorId, dependencyTypeName, dependencyName, target, data, startTime,
                    durationMilliseconds, errorType, errorMessage, stackTrace, managedThreadId, isSuccess);
        }
    }
}