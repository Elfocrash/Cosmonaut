using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;

namespace Cosmonaut.Tests
{
    public class MockHelpers
    {
        public static Mock<IDocumentClient> GetFakeDocumentClient(string databaseName = "databaseName")
        {
            var mockDocumentClient = new Mock<IDocumentClient>();
            var mockDatabase = new Mock<Database>();
            var mockCollection = new Mock<DocumentCollection>();
            var mockOffer = new Mock<Offer>();
            mockDatabase.Setup(x => x.Id).Returns(databaseName);
            mockCollection.Setup(x => x.Id).Returns("dummies");
            mockDocumentClient.Setup(x => x.AuthKey).Returns(new SecureString());
            mockDocumentClient.Setup(x => x.ServiceEndpoint).Returns(new Uri("http://test.com"));
            mockDocumentClient.Setup(x => x.ReadDatabaseAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new ResourceResponse<Database>(mockDatabase.Object));
            mockDocumentClient.Setup(x => x.ReadDocumentCollectionAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new ResourceResponse<DocumentCollection>(mockCollection.Object));
            mockDocumentClient.Setup(x => x.CreateDatabaseQuery(null))
                .Returns(new EnumerableQuery<Database>(new List<Database>() { mockDatabase.Object }));
            mockDocumentClient.Setup(x => x.CreateDocumentCollectionQuery(It.IsAny<string>(), null))
                .Returns(new EnumerableQuery<DocumentCollection>(new List<DocumentCollection>() { new DocumentCollection() { Id = "dummies" } }));
            mockDocumentClient.Setup(x => x.CreateOfferQuery(null)).Returns(
                new EnumerableQuery<OfferV2>(new List<OfferV2>()
                {
                    new OfferV2(mockOffer.Object, 400)
                }));
            return mockDocumentClient;
        }
    }
}