using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Testing
{
    public static class TestingExtensions
    {
        public static ResourceResponse<T> ToResourceResponse<T>(this T resource, HttpStatusCode statusCode, IDictionary<string, string> responseHeaders = null) where T : Resource, new()
        {
            var resourceResponse = new ResourceResponse<T>(resource);
            var documentServiceResponseType = Type.GetType("Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=1.9.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            var flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            if (responseHeaders != null)
            {
                foreach (var responseHeader in responseHeaders)
                {
                    headers[responseHeader.Key] = responseHeader.Value;
                }
            }

            var arguments = new object[] { Stream.Null, headers, statusCode, null };

            var documentServiceResponse =
                documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<T>).GetTypeInfo().GetField("response", BindingFlags.NonPublic | BindingFlags.Instance);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            return resourceResponse;
        }


        public static FeedResponse<T> ToFeedResponse<T>(this IQueryable<T> resource, IDictionary<string, string> responseHeaders = null)
        {
            var feedResponseType = Type.GetType("Microsoft.Azure.Documents.Client.FeedResponse`1, Microsoft.Azure.DocumentDB.Core, Version=1.9.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            var flags = BindingFlags.NonPublic | BindingFlags.Instance;

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

            var arguments = new object[] { resource, resource.Count(), headers, false, null };

            if (feedResponseType != null)
            {
                var t = feedResponseType.MakeGenericType(typeof(T));

                var feedResponse = t.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

                return (FeedResponse<T>)feedResponse;
            }

            return new FeedResponse<T>();
        }
    }
}