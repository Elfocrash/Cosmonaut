using System.Net;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using Cosmonaut.Testing;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Cosmonaut.Unit
{
    public class ServiceCollectionTests
    {
        [Fact]
        public void AddCosmosStoreRegistersStore()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var documentClient = MockHelpers.GetMockDocumentClient();
            var databaseResource = new Database { Id = "databaseName" }.ToResourceResponse(HttpStatusCode.OK);
            documentClient.Setup(x => x.ReadDatabaseAsync(UriFactory.CreateDatabaseUri("databaseName"), It.IsAny<RequestOptions>()))
                .ReturnsAsync(databaseResource);

            // Act
            serviceCollection.AddCosmosStore<Dummy>(new CosmonautClient(documentClient.Object), "databaseName");
            var provider = serviceCollection.BuildServiceProvider();

            // Assert
            var cosmosStore = provider.GetService<ICosmosStore<Dummy>>();
            Assert.NotNull(cosmosStore);
        }

        [Fact]
        public void AddCosmosStoreWithCosmonautClientRegistersStore()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var cosmonautClient = SetupCosmonautClient();

            // Act
            serviceCollection.AddCosmosStore<Dummy>(cosmonautClient, "databaseName");
            var provider = serviceCollection.BuildServiceProvider();

            // Assert
            var cosmosStore = provider.GetService<ICosmosStore<Dummy>>();
            Assert.NotNull(cosmosStore);
        }
        
        private static CosmonautClient SetupCosmonautClient()
        {
            var documentClient = MockHelpers.GetMockDocumentClient();
            var cosmonautClient = new CosmonautClient(documentClient.Object);
            var databaseResource = new Database {Id = "databaseName"}.ToResourceResponse(HttpStatusCode.OK);

            documentClient.Setup(x =>
                    x.ReadDatabaseAsync(UriFactory.CreateDatabaseUri("databaseName"), It.IsAny<RequestOptions>()))
                .ReturnsAsync(databaseResource);
            return cosmonautClient;
        }
    }
}