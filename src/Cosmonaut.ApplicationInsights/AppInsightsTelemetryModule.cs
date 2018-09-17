using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using Cosmonaut.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Cosmonaut.ApplicationInsights
{
    public sealed class AppInsightsTelemetryModule : EventListener, ITelemetryModule
    {
        private static readonly Lazy<ITelemetryModule> SingleInstance =
            new Lazy<ITelemetryModule>(() => new AppInsightsTelemetryModule(), LazyThreadSafetyMode.ExecutionAndPublication);

        private TelemetryClient _telemetryClient;
        private bool _initialised;

        private AppInsightsTelemetryModule()
        {
        }

        public static ITelemetryModule Instance => SingleInstance.Value;

        public void Initialize(TelemetryConfiguration configuration)
        {
            if (_initialised)
            {
                return;
            }

            _telemetryClient =
                new TelemetryClient(configuration) {InstrumentationKey = configuration.InstrumentationKey};
            _initialised = true;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            base.OnEventSourceCreated(eventSource);

            if (eventSource.IsDependencyTrackingEventSource())
            {
                EnableEvents(eventSource, EventLevel.Informational);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (!_initialised || !eventData.EventSource.IsDependencyTrackingEventSource())
            {
                return;
            }

            var dependencyTelemetry = CreateDependency(eventData);
            _telemetryClient.TrackDependency(dependencyTelemetry);
        }

        private static DependencyTelemetry CreateDependency(EventWrittenEventArgs eventData)
        {
            var dependency = AsDependency(eventData);

            var dependencyTelemetry = new DependencyTelemetry(
                dependency.DependencyTypeName,
                dependency.Target,
                dependency.DependencyName,
                dependency.Data,
                dependency.StartTime,
                dependency.Duration,
                dependency.ResultCode,
                dependency.Success);

            foreach (var propertyPair in dependency.Properties ?? new Dictionary<string, object>())
            {
                dependencyTelemetry.Context.Properties[propertyPair.Key] = propertyPair.Value?.ToString() ?? string.Empty;
            }

            return dependencyTelemetry;
        }

        public static CosmosEventMetadata AsDependency(EventWrittenEventArgs eventData)
        {
            return CosmosEventDataConverter.ExtractData(eventData).AsDependency();
        }

        public static CosmosEventMetadata AsDependency(IDictionary<string, object> eventData)
        {
            return CosmosEventDataConverter.ConvertToDependencyFromEventData(eventData);
        }
    }
}