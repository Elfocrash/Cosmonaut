using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Testing
{
    public static class TestingExtensions
    {
        public static ResourceResponse<T> ToResourceResponse<T>(this T resource, HttpStatusCode statusCode) where T : Resource, new()
        {
            var resourceResponse = new ResourceResponse<T>(resource);
            var documentServiceResponseType = Type.GetType("Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=1.9.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            var flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };


            var arguments = new object[] { Stream.Null, headers, statusCode, null };

            var documentServiceResponse = Activator.CreateInstance(documentServiceResponseType ?? throw new InvalidOperationException(), flags, null, arguments, null);

            var responseField = typeof(ResourceResponse<T>).GetField("response", BindingFlags.NonPublic | BindingFlags.Instance);

            if (responseField != null) responseField.SetValue(resourceResponse, documentServiceResponse);

            return resourceResponse;
        }
    }
}