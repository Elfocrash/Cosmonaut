using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Cosmonaut.Tests
{
    public class ServiceCollectionTests
    {
        [Fact]
        public void AddCosmosStoreRegistersStore()
        {
            // Assign
            var serviceCollection = new ServiceCollection();
            var mockDocumentClient = new Mock<IDocumentClient>();
            var mockDatabase = new Mock<Database>();
            var mockCollection = new Mock<DocumentCollection>();
            var mockOffer = new Mock<Offer>();
            mockDatabase.Setup(x => x.Id).Returns("databaseName");
            mockCollection.Setup(x => x.Id).Returns("dummies");

            mockDocumentClient.Setup(x => x.AuthKey).Returns(new SecureString());
            mockDocumentClient.Setup(x => x.ServiceEndpoint).Returns(new Uri("http://test.com"));
            mockDocumentClient.Setup(x => x.ReadDatabaseAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new ResourceResponse<Database>(mockDatabase.Object));
            mockDocumentClient.Setup(x => x.ReadDocumentCollectionAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new ResourceResponse<DocumentCollection>(mockCollection.Object));
            mockDocumentClient.Setup(x => x.CreateDatabaseQuery(null))
                .Returns(new EnumerableQuery<Database>(new List<Database>(){mockDatabase.Object}));
            mockDocumentClient.Setup(x => x.CreateDocumentCollectionQuery(It.IsAny<string>(), null))
                .Returns(new EnumerableQuery<DocumentCollection>(new List<DocumentCollection>() {new DocumentCollection(){Id = "dummies" } }));
            mockDocumentClient.Setup(x => x.CreateOfferQuery(null)).Returns(
                new EnumerableQuery<OfferV2>(new List<OfferV2>()
                {
                    new OfferV2(mockOffer.Object, 400)
                }));

            //Act
            serviceCollection.AddCosmosStore<Dummy>(mockDocumentClient.Object, "databaseName");
            var provider = serviceCollection.BuildServiceProvider();

            //Assert
            var cosmosStore = provider.GetService<ICosmosStore<Dummy>>();
            Assert.NotNull(cosmosStore);
        }
    }
}