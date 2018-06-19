using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Cosmonaut.Diagnostics
{
    public class CosmosEventCall
    {
        public CosmosEventMetadata EventMetadata { get; }

        public CosmosEventCall(CosmosEventMetadata eventMetadata)
        {
            EventMetadata = eventMetadata;
        }

        public async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> eventCall)
        {
            var timer = new Stopwatch();
            try
            {
                EventMetadata.StartTime = DateTimeOffset.UtcNow;
                SetDependencyNameInCaseOfAbsence(eventCall);
                timer.Start();
                var result = await eventCall();
                timer.Stop();
                TrackSuccess(timer);
                return result;
            }
            catch (Exception ex)
            {
                timer.Stop();
                TrackException(ex, timer);
                throw;
            }
        }
        
        public async Task<FeedResponse<TEntity>> InvokeAsync<TEntity>(Func<Task<FeedResponse<TEntity>>> eventCall)
        {
            var timer = new Stopwatch();
            try
            {
                EventMetadata.StartTime = DateTimeOffset.UtcNow;
                SetDependencyNameInCaseOfAbsence(eventCall);
                timer.Start();
                var result = await eventCall();
                timer.Stop();
                AddQueryMetricsInTheProperties(result);
                EventMetadata.Properties[nameof(result.RequestCharge)] = result.RequestCharge;
                TrackSuccess(timer);
                return result;
            }
            catch (Exception ex)
            {
                timer.Stop();
                TrackException(ex, timer);
                throw;
            }
        }

        private void AddQueryMetricsInTheProperties<TEntity>(FeedResponse<TEntity> result)
        {
            if (result.QueryMetrics != null)
                EventMetadata.Properties[nameof(result.QueryMetrics)] = JsonConvert.SerializeObject(result.QueryMetrics);
        }

        private void TrackException(Exception ex, Stopwatch timer)
        {
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
                EventMetadata.Success,
                JsonConvert.SerializeObject(EventMetadata.Properties));
        }

        private void TrackSuccess(Stopwatch timer)
        {
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
                EventMetadata.Success,
                JsonConvert.SerializeObject(EventMetadata.Properties));
        }

        private void SetDependencyNameInCaseOfAbsence<TResult>(Func<Task<TResult>> eventCall)
        {
            if (string.IsNullOrEmpty(EventMetadata.DependencyName))
                EventMetadata.DependencyName = eventCall.GetMethodInfo().Name;
        }
    }
}