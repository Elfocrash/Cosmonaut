using System;
using System.Text;
using Cosmonaut.Extensions;
using Cosmonaut.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Cosmonaut.Tests
{
    public class ServiceCollectionTests
    {
        [Fact]
        public void AddCosmosStoreRegistersStore()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var documentClient = MockHelpers.GetMockDocumentClient();

            // Act
            serviceCollection.AddCosmosStore<Dummy>(documentClient.Object, "databaseName", new CosmosDatabaseCreator(documentClient.Object), new CosmosCollectionCreator(documentClient.Object));
            var provider = serviceCollection.BuildServiceProvider();

            // Assert
            var cosmosStore = provider.GetService<ICosmosStore<Dummy>>();
            Assert.NotNull(cosmosStore);
        }
    }
}