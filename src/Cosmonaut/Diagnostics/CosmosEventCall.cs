using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private static readonly IDictionary<string, string> TrackedResponseHeaders = new Dictionary<string, string>{
            {"etag", "Etag" },
            {"x-ms-activity-id", "ActivityId"},
            {"x-ms-alt-content-path", "AltContentPath"},
            {"x-ms-continuation", "RequestContinuation"},
            {"x-ms-item-count", "ItemCount"},
            {"x-ms-request-charge", "RequestCharge"},
            {"x-ms-resource-quota", "ResourceQuota"},
            {"x-ms-resource-usage", "ResourceUsage"},
            {"x-ms-schemaversion", "SchemaVersion"},
            {"x-ms-serviceversion", "ServiceVersion"},
            {"x-ms-session-token", "SessionToken"},
            {"x-ms-lsn", "Lsn"},
            {"x-ms-quorum-acked-lsn", "QuorumAckedLsn"},
            {"x-ms-current-write-quorum", "CurrentWriteQuorum"},
            {"x-ms-current-replica-set-size", "CurrentReplicaSetSize"},
            {"x-ms-global-Committed-lsn", "GlobalCommittedLsn"}
        };

        internal CosmosEventMetadata EventMetadata { get; }

        internal CosmosEventCall(CosmosEventMetadata eventMetadata)
        {
            EventMetadata = eventMetadata;
        }

        internal async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> eventCall)
        {
            if (!CosmosEventSource.EventSource.IsEnabled())
            {
                return await eventCall();
            }

            var timer = new Stopwatch();
            try
            {
                SetPreExecutionEventMetadata(eventCall);
                timer.Start();
                var result = await eventCall();
                timer.Stop();
                TrackSuccess(timer, HttpStatusCode.OK.ToString("D"));
                return result;
            }
            catch (Exception ex)
            {
                timer.Stop();
                TrackException(ex, timer);
                throw;
            }
        }

        internal async Task<FeedResponse<TEntity>> InvokeAsync<TEntity>(Func<Task<FeedResponse<TEntity>>> eventCall)
        {
            if (!CosmosEventSource.EventSource.IsEnabled())
            {
                return await eventCall();
            }

            var timer = new Stopwatch();
            try
            {
                SetPreExecutionEventMetadata(eventCall);
                timer.Start();
                var result = await eventCall();
                timer.Stop();
                AddEventMetadataFromHeaders(result.ResponseHeaders);
                LogQueryMetricsIfPresent(result);
                TrackSuccess(timer, HttpStatusCode.OK.ToString("D"));
                return result;
            }
            catch (Exception ex)
            {
                timer.Stop();
                TrackException(ex, timer);
                throw;
            }
        }

        internal async Task<DocumentResponse<TEntity>> InvokeAsync<TEntity>(Func<Task<DocumentResponse<TEntity>>> eventCall)
        {
            if (!CosmosEventSource.EventSource.IsEnabled())
            {
                return await eventCall();
            }

            var timer = new Stopwatch();
            try
            {
                SetPreExecutionEventMetadata(eventCall);
                timer.Start();
                var result = await eventCall();
                timer.Stop();
                AddEventMetadataFromHeaders(result.ResponseHeaders);
                TrackSuccess(timer, HttpStatusCode.OK.ToString("D"));
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
            if (!CosmosEventSource.EventSource.IsEnabled())
            {
                return await eventCall();
            }

            var timer = new Stopwatch();
            try
            {
                SetPreExecutionEventMetadata(eventCall);
                timer.Start();
                var result = await eventCall();
                timer.Stop();
                AddEventMetadataFromHeaders(result.ResponseHeaders);
                TrackSuccess(timer, result.StatusCode.ToString("D"));
                return result;
            }
            catch (Exception ex)
            {
                timer.Stop();
                TrackException(ex, timer);
                throw;
            }
        }

        internal async Task<StoredProcedureResponse<TEntity>> InvokeAsync<TEntity>(Func<Task<StoredProcedureResponse<TEntity>>> eventCall)
        {
            if (!CosmosEventSource.EventSource.IsEnabled())
            {
                return await eventCall();
            }

            var timer = new Stopwatch();
            try
            {
                SetPreExecutionEventMetadata(eventCall);
                timer.Start();
                var result = await eventCall();
                timer.Stop();
                AddEventMetadataFromHeaders(result.ResponseHeaders);
                TrackSuccess(timer, HttpStatusCode.OK.ToString("D"));
                return result;
            }
            catch (Exception ex)
            {
                timer.Stop();
                AddEventMetadataFromException(ex);
                TrackException(ex, timer);
                throw;
            }
        }

        private void LogQueryMetricsIfPresent<TEntity>(FeedResponse<TEntity> result)
        {
            if (result.QueryMetrics == null)
                return;

            EventMetadata.Properties[nameof(result.QueryMetrics)] = JsonConvert.SerializeObject(result.QueryMetrics);
        }

        private void SetPreExecutionEventMetadata<TResult>(Func<Task<TResult>> eventCall)
        {
            EventMetadata.StartTime = DateTimeOffset.UtcNow;
            if (string.IsNullOrEmpty(EventMetadata.DependencyName))
                EventMetadata.DependencyName = eventCall.GetMethodInfo().Name;
        }

        private void TrackException(Exception ex, Stopwatch timer)
        {
            EventMetadata.Error = ex;
            EventMetadata.Duration = timer.Elapsed;
            EventMetadata.Success = false;
            AddEventMetadataFromException(ex);

            CosmosEventSource.EventSource.TrackError(
                EventMetadata.DependencyTypeName,
                EventMetadata.DependencyName,
                EventMetadata.Target ?? EventMetadata.DependencyName,
                EventMetadata.ResultCode,
                EventMetadata.Data,
                EventMetadata.StartTime.ToUnixTimeMilliseconds(),
                EventMetadata.Duration.TotalMilliseconds,
                ex.GetType().FullName,
                ex.Message,
                ex.StackTrace,
                EventMetadata.Success,
                JsonConvert.SerializeObject(EventMetadata.Properties));
        }

        private void TrackSuccess(Stopwatch timer, string resultCode)
        {
            EventMetadata.Success = true;
            EventMetadata.Duration = timer.Elapsed;
            EventMetadata.ResultCode = resultCode;

            CosmosEventSource.EventSource.TrackSuccess(
                EventMetadata.DependencyTypeName,
                EventMetadata.DependencyName,
                EventMetadata.Target ?? EventMetadata.DependencyName,
                EventMetadata.ResultCode,
                EventMetadata.Data,
                EventMetadata.StartTime.ToUnixTimeMilliseconds(),
                EventMetadata.Duration.TotalMilliseconds,
                EventMetadata.Success,
                JsonConvert.SerializeObject(EventMetadata.Properties));
        }
        
        private void AddEventMetadataFromException(Exception ex)
        {
            if (!(ex is DocumentClientException documentClientException))
            {
                EventMetadata.ResultCode = HttpStatusCode.InternalServerError.ToString("D");
                return;
            }
                
            AddEventMetadataFromHeaders(documentClientException.ResponseHeaders);
            EventMetadata.ResultCode = documentClientException.StatusCode?.ToString("D") ?? HttpStatusCode.InternalServerError.ToString("D");
        }

        private void AddEventMetadataFromHeaders(NameValueCollection headers)
        {
            if (headers == null)
                return;

            if (string.IsNullOrEmpty(EventMetadata.Target))
                EventMetadata.Target = headers.Get("x-ms-alt-content-path");

            foreach (string header in headers)
            {
                if (!TrackedResponseHeaders.ContainsKey(header))
                    continue;

                EventMetadata.Properties[TrackedResponseHeaders[header]] = headers[header];
            }
        }
    }
}