using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Cosmonaut.Diagnostics
{
    internal class CosmosEventCall
    {
        internal CosmosEventMetadata EventMetadata { get; }

        internal CosmosEventCall(CosmosEventMetadata eventMetadata)
        {
            EventMetadata = eventMetadata;
        }

        internal async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> eventCall)
        {
            var timer = new Stopwatch();
            try
            {
                EventMetadata.StartTime = DateTimeOffset.UtcNow;
                if (string.IsNullOrEmpty(EventMetadata.DependencyName))
                    EventMetadata.DependencyName = eventCall.GetMethodInfo().Name;
                timer.Start();
                var result = await eventCall();
                timer.Stop();
                TrackSuccess(timer);
                return result;
            }
            catch (Exception ex)
            {
                timer.Stop();
                EventMetadata.ResultCode = HttpStatusCode.InternalServerError.ToString("D");
                TrackException(ex, timer);
                throw;
            }
        }

        internal async Task<FeedResponse<TEntity>> InvokeAsync<TEntity>(Func<Task<FeedResponse<TEntity>>> eventCall)
        {
            var timer = new Stopwatch();
            try
            {
                EventMetadata.StartTime = DateTimeOffset.UtcNow;
                if (string.IsNullOrEmpty(EventMetadata.DependencyName))
                    EventMetadata.DependencyName = eventCall.GetMethodInfo().Name;
                timer.Start();
                var result = await eventCall();
                timer.Stop();
                SetFeedResponseProperties(result);
                TrackSuccess(timer);
                return result;
            }
            catch (Exception ex)
            {
                timer.Stop();
                EventMetadata.ResultCode = HttpStatusCode.InternalServerError.ToString("D");
                TrackException(ex, timer);
                throw;
            }
        }

        internal async Task<ResourceResponse<TEntity>> InvokeAsync<TEntity>(Func<Task<ResourceResponse<TEntity>>> eventCall) where TEntity : Resource, new()
        {
            var timer = new Stopwatch();
            try
            {
                EventMetadata.StartTime = DateTimeOffset.UtcNow;
                if (string.IsNullOrEmpty(EventMetadata.DependencyName))
                    EventMetadata.DependencyName = eventCall.GetMethodInfo().Name;
                timer.Start();
                var result = await eventCall();
                timer.Stop();
                SetResourceResponseProperties(result);
                TrackSuccess(timer);
                return result;
            }
            catch (Exception ex)
            {
                timer.Stop();
                EventMetadata.ResultCode = HttpStatusCode.InternalServerError.ToString("D");
                TrackException(ex, timer);
                throw;
            }
        }
        
        private void SetFeedResponseProperties<TEntity>(FeedResponse<TEntity> result)
        {
            if (result.QueryMetrics != null)
            {
                EventMetadata.Properties[nameof(result.QueryMetrics)] = JsonConvert.SerializeObject(result.QueryMetrics);
            }

            EventMetadata.Properties[nameof(result.Count)] = result.Count.ToString();
            EventMetadata.Properties[nameof(result.ActivityId)] = result.ActivityId;
            EventMetadata.Properties[nameof(result.RequestCharge)] = result.RequestCharge;
            EventMetadata.ResultCode = HttpStatusCode.OK.ToString("D");
        }

        private void SetResourceResponseProperties<TEntity>(ResourceResponse<TEntity> result) where TEntity : Resource, new()
        {
            if (result == null)
                return;

            EventMetadata.ResultCode = result.StatusCode.ToString("D");
            EventMetadata.Properties[nameof(result.RequestCharge)] = result.RequestCharge;
            EventMetadata.Properties[nameof(result.ActivityId)] = result.ActivityId;
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
    }
}