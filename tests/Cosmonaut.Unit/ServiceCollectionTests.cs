using Cosmonaut.Extensions;
using Microsoft.Extensions.DependencyInjection;
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

            // Act
            serviceCollection.AddCosmosStore<Dummy>(documentClient.Object, "databaseName", "", "http://test.com");
            var provider = serviceCollection.BuildServiceProvider();

            // Assert
            var cosmosStore = provider.GetService<ICosmosStore<Dummy>>();
            Assert.NotNull(cosmosStore);
        }
    }
}