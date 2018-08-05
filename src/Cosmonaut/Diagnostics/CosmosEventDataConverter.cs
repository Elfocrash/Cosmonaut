using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Newtonsoft.Json;

namespace Cosmonaut.Diagnostics
{
    public static class CosmosEventDataConverter
    {
        public static CosmosEventMetadata ConvertToDependencyFromEventData(IDictionary<string, object> eventData)
        {
            var duration = TranslateDuration(eventData);
            var startTime = TranslateStartTime(eventData);
            var resultCode = TranslateResultCode(eventData);
            var dependencyTypeName = TranslateDependencyTypeName(eventData);
            var dependencyName = TranslateDependencyName(eventData);
            var target = TranslateTarget(eventData);
            var evData = TranslateData(eventData);
            var success = TranslateSuccess(eventData);
            var errorMessage = TranslateErrorMessage(eventData);
            var stackTrace = TranslateStackTrace(eventData);
            var properties = TranslateProperties(eventData);
            var dep = CreateCosmosDependencyMetadata( evData, dependencyName, dependencyTypeName, target, resultCode, duration, startTime, success, properties);
            SetErrorIfExists(errorMessage, dep, stackTrace);
            return dep;
        }

        private static CosmosEventMetadata CreateCosmosDependencyMetadata(
            object evData, 
            object dependencyName, 
            object dependencyTypeName, 
            object target, 
            object resultCode, 
            TimeSpan duration, 
            DateTimeOffset startTime, 
            bool success,
            IDictionary<string, object> properties)
        {
            return new CosmosEventMetadata
            {
                Data = evData.ToString(),
                DependencyName = dependencyName.ToString(),
                DependencyTypeName = dependencyTypeName.ToString(),
                Target = target.ToString(),
                ResultCode = resultCode.ToString(),
                Duration = duration,
                StartTime = startTime,
                Success = success,
                Properties = properties
            };
        }

        private static void SetErrorIfExists(object errorMessage, CosmosEventMetadata dep, object stackTrace)
        {
            if (!string.IsNullOrEmpty(errorMessage.ToString()))
                dep.Error = new Exception($"{errorMessage} {stackTrace}");
        }

        private static object TranslateStackTrace(IDictionary<string, object> eventData)
        {
            if (!eventData.TryGetValue("stackTrace", out var stackTrace)) stackTrace = string.Empty;
            return stackTrace;
        }

        private static object TranslateErrorMessage(IDictionary<string, object> eventData)
        {
            if (!eventData.TryGetValue("errorMessage", out var errorMessage)) errorMessage = string.Empty;
            return errorMessage;
        }

        public static IDictionary<string, object> ExtractData(EventWrittenEventArgs eventData)
        {
            return eventData
                .PayloadNames
                .Zip(eventData.Payload, (k, v) => new { k, v })
                .ToDictionary(x => x.k, x => x.v);
        }

        private static bool TranslateSuccess(IDictionary<string, object> eventData)
        {
            bool success;
            if (eventData.TryGetValue("isSuccess", out var successVal) && successVal is bool b)
                success = b;
            else
                success = true;
            return success;
        }

        private static IDictionary<string, object> TranslateProperties(IDictionary<string, object> eventData)
        {
            if (eventData.TryGetValue("properties", out var properties) && properties is string serialisedProperties)
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(serialisedProperties);
            }

            return new Dictionary<string, object>();
        }

        private static object TranslateData(IDictionary<string, object> eventData)
        {
            if (!eventData.TryGetValue("data", out var evData)) evData = string.Empty;
            return evData;
        }

        private static object TranslateTarget(IDictionary<string, object> eventData)
        {
            if (!eventData.TryGetValue("target", out var target)) target = "unknown";
            return target;
        }

        private static object TranslateDependencyName(IDictionary<string, object> eventData)
        {
            if (!eventData.TryGetValue("dependencyName", out var dependencyName)) dependencyName = "unknown";
            return dependencyName;
        }

        private static object TranslateDependencyTypeName(IDictionary<string, object> eventData)
        {
            if (!eventData.TryGetValue("dependencyTypeName", out var dependencyTypeName))
                dependencyTypeName = "unknown";
            return dependencyTypeName;
        }

        private static object TranslateResultCode(IDictionary<string, object> eventData)
        {
            if (!eventData.TryGetValue("resultCode", out var resultCode)) resultCode = string.Empty;
            return resultCode;
        }

        private static DateTimeOffset TranslateStartTime(IDictionary<string, object> eventData)
        {
            DateTimeOffset startTime;
            if (eventData.TryGetValue("startTime", out var startTimeObj) && startTimeObj is long l)
                startTime = DateTimeOffset.FromUnixTimeMilliseconds(l);
            else
                startTime = DateTimeOffset.UtcNow;
            return startTime;
        }

        private static TimeSpan TranslateDuration(IDictionary<string, object> eventData)
        {
            var duration = TimeSpan.Zero;
            if (eventData.TryGetValue("durationMilliseconds", out var durationVal))
                duration = TimeSpan.FromMilliseconds((double) durationVal);
            return duration;
        }
    }
}