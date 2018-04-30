using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;

namespace Cosmonaut.Tests
{
    public static class MockHelpers
    {
        public static Mock<IDocumentClient> GetMockDocumentClient(string databaseName = "databaseName")
        {
            var mockDocumentClient = new Mock<IDocumentClient>();
            var mockDatabase = new Mock<Database>();
            var mockCollection = GetMockDocumentCollection();
            var mockOffer = new Mock<Offer>();
            mockOffer.Object.SetPropertyValue("resource", "docs");
            mockOffer.Object.SetResourceTimestamp(DateTime.UtcNow);
            mockDatabase.Setup(x => x.Id).Returns(databaseName);
            mockDocumentClient.Setup(x => x.AuthKey).Returns(new SecureString());
            mockDocumentClient.Setup(x => x.ServiceEndpoint).Returns(new Uri("http://test.com"));
            mockDocumentClient.Setup(x => x.ReadDatabaseAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new ResourceResponse<Database>(mockDatabase.Object));
            mockDocumentClient.Setup(x => x.ReadDocumentCollectionAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new ResourceResponse<DocumentCollection>(mockCollection.Object));
            mockDocumentClient.Setup(x => x.CreateDatabaseQuery(null))
                .Returns(new EnumerableQuery<Database>(new List<Database> { mockDatabase.Object }));
            mockDocumentClient.Setup(x => x.CreateDocumentCollectionQuery(It.IsAny<string>(), null))
                .Returns(new EnumerableQuery<DocumentCollection>(new List<DocumentCollection> { mockCollection.Object }));
            var offerV2 = new OfferV2(mockOffer.Object, 400);
            offerV2.SetResourceTimestamp(DateTime.UtcNow);
            mockDocumentClient.Setup(x => x.CreateOfferQuery(null)).Returns(
                new EnumerableQuery<OfferV2>(new List<OfferV2>
                {
                    offerV2
                }));
            return mockDocumentClient;
        }

        public static Mock<DocumentCollection> GetMockDocumentCollection()
        {
            var mockCollection = new Mock<DocumentCollection>();
            mockCollection.Setup(x => x.Id).Returns("dummies");
            mockCollection.Object.SetPropertyValue("resource", "docs");
            mockCollection.Object.SetPropertyValue("_self", "docs");
            return mockCollection;
        }

        public static ResourceResponse<T> CreateResourceResponse<T>(T resource, HttpStatusCode statusCode) where T : Resource, new()
        {
            resource.SetResourceTimestamp(DateTime.UtcNow);
            var resourceResponse = new ResourceResponse<T>(resource);
            var documentServiceResponseType = Type.GetType("Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=1.10.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            var flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection {{"x-ms-request-charge", "0"}};


            var arguments = new object[] { Stream.Null, headers, statusCode, null };
            
            var documentServiceResponse = Activator.CreateInstance(documentServiceResponseType, flags, null, arguments, null);
            
            var responseField = typeof(ResourceResponse<T>).GetField("response", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            responseField.SetValue(resourceResponse, documentServiceResponse);

            return resourceResponse;
        }

        public static void SetResourceTimestamp<T>(this T resource, DateTime dateTime) where T : Resource
        {
            resource?.SetPropertyValue("_ts", (object)(ulong)(DateTime.UtcNow - UnixStartTime).TotalSeconds);
        }

        private static readonly DateTime UnixStartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    }
}