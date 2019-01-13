using System;
using System.Diagnostics.Tracing;
using System.Threading;
using Cosmonaut.Diagnostics;
using Newtonsoft.Json;
using Serilog;

namespace Cosmonaut.Console
{
    public class SerilogEventListener : EventListener
    {
        private static readonly Lazy<SerilogEventListener> SingleInstance =
            new Lazy<SerilogEventListener>(() => new SerilogEventListener(), LazyThreadSafetyMode.ExecutionAndPublication);

        private bool _initialised;

        private SerilogEventListener()
        {
        }

        public static SerilogEventListener Instance => SingleInstance.Value;

        public void Initialize()
        {
            if (_initialised)
            {
                return;
            }

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

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

            var dependencyTelemetry = AsDependency(eventData);
            Log.Logger.Information(JsonConvert.SerializeObject(dependencyTelemetry));
        }

        public static CosmosEventMetadata AsDependency(EventWrittenEventArgs eventData)
        {
            return CosmosEventDataConverter.ExtractData(eventData).AsDependency();
        }
    }
}