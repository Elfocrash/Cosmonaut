using Cosmonaut.Attributes;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosCollectionAttributeTests
    {
        [Fact]
        public void SettingTheNameSetsNameAndDefaultThroughput()
        {
            // Arrange
            var expectedThroughput = -1;
            var expectedName = "name";

            // Act
            var collectionAttribute = new CosmosCollectionAttribute("name");

            // Assert
            Assert.Equal(expectedThroughput, collectionAttribute.Throughput);
            Assert.Equal(expectedName, collectionAttribute.Name);
        }

        [Fact]
        public void SettingTheNameAndAttributeSetsNameAndDefaultThroughput()
        {
            // Arrange
            var expectedThroughput = 1000;
            var expectedName = "name";

            // Act
            var collectionAttribute = new CosmosCollectionAttribute("name", 1000);

            // Assert
            Assert.Equal(expectedThroughput, collectionAttribute.Throughput);
            Assert.Equal(expectedName, collectionAttribute.Name);
        }

        [Fact]
        public void EmptyCtorCosmosCollectionAttributeDefaults()
        {
            // Arrange
            var expectedThroughput = -1;

            // Act
            var collectionAttribute = new CosmosCollectionAttribute();

            // Assert
            Assert.Equal(expectedThroughput, collectionAttribute.Throughput);
            Assert.Null(collectionAttribute.Name);
        }
    }
}