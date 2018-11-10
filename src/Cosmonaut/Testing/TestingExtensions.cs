using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Cosmonaut.Internal;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Testing
{
    public static class TestingExtensions
    {
        public static ResourceResponse<T> ToResourceResponse<T>(this T resource, HttpStatusCode statusCode, IDictionary<string, string> responseHeaders = null) where T : Resource, new()
        {
            var resourceResponse = new ResourceResponse<T>(resource);

            var flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            if (responseHeaders != null)
            {
                foreach (var responseHeader in responseHeaders)
                {
                    headers[responseHeader.Key] = responseHeader.Value;
                }
            }

            var headersDictionaryInstance = Activator.CreateInstance(InternalTypeCache.Instance.DictionaryNameValueCollectionType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, statusCode, null };

            var documentServiceResponse = InternalTypeCache.Instance.DocumentServiceResponseCtorInfo.Invoke(arguments);

            var responseField = typeof(ResourceResponse<T>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            return resourceResponse;
        }

        public static FeedResponse<T> ToFeedResponse<T>(this IQueryable<T> resource, IDictionary<string, string> responseHeaders = null)
        {
            var headers = new NameValueCollection
            {
                { "x-ms-request-charge", "0" },
                { "x-ms-activity-id", Guid.NewGuid().ToString() }
            };

            if (responseHeaders != null)
            {
                foreach (var responseHeader in responseHeaders)
                {
                    headers[responseHeader.Key] = responseHeader.Value;
                }
            }

            var headersDictionaryInstance = Activator.CreateInstance(InternalTypeCache.Instance.DictionaryNameValueCollectionType, headers);

            var arguments = new[] { resource, resource.Count(), headersDictionaryInstance, false, null, null, null, 0 };

            var feedResponse = InternalTypeCache.Instance.FeedResponseCtorInfo<T>().Invoke(arguments);

            return (FeedResponse<T>)feedResponse;
        }
    }
}