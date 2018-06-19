using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cosmonaut.Diagnostics
{
    public class CosmosEventCall<TResult>
    {
        private readonly Func<Task<TResult>> _eventCall;
        public CosmosEventMetadata EventMetadata { get; }

        public CosmosEventCall(Func<Task<TResult>> eventCall, CosmosEventMetadata eventMetadata)
        {
            _eventCall = eventCall ?? throw new ArgumentNullException(nameof(eventCall));
            EventMetadata = eventMetadata;
        }

        public async Task<TResult> InvokeAsync()
        {
            var timer = new Stopwatch();

            try
            {
                EventMetadata.StartTime = DateTimeOffset.UtcNow;
                timer.Start();
                var result = await _eventCall();
                timer.Stop();
                EventMetadata.Success = true;
                EventMetadata.Duration = timer.Elapsed;

                CosmosEventSource.EventSource.TrackSuccess(
                    EventMetadata.DependencyTypeName,
                    EventMetadata.DependencyName,
                    EventMetadata.Target,
                    EventMetadata.ResultCode,
                    EventMetadata.Data,
                    EventMetadata.StartTime.ToUnixTimeMilliseconds(),
                    EventMetadata.Duration.TotalMilliseconds,
                    EventMetadata.ManagedThreadId,
                    EventMetadata.Success,
                    JsonConvert.SerializeObject(EventMetadata.Properties ?? new Dictionary<string, string>()));

                return result;
            }
            catch (Exception ex)
            {
                timer.Stop();

                EventMetadata.Error = ex;
                EventMetadata.Duration = timer.Elapsed;
                EventMetadata.Success = false;

                CosmosEventSource.EventSource.TrackError(
                    EventMetadata.DependencyTypeName,
                    EventMetadata.DependencyName,
                    EventMetadata.Target,
                    EventMetadata.Data,
                    EventMetadata.StartTime.ToUnixTimeMilliseconds(),
                    EventMetadata.Duration.TotalMilliseconds,
                    ex.GetType().FullName,
                    ex.Message,
                    ex.StackTrace,
                    EventMetadata.ManagedThreadId,
                    EventMetadata.Success,
                    JsonConvert.SerializeObject(EventMetadata.Properties ?? new Dictionary<string, string>()));

                throw;
            }
        }
    }
}