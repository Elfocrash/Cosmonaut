using Cosmonaut.Attributes;
using Xunit;

namespace Cosmonaut.Unit
{
    public class CosmosCollectionAttributeTests
    {
        [Fact]
        public void SettingTheNameSetsNameAndDefaultThroughput()
        {
            // Arrange
            var expectedName = "name";

            // Act
            var collectionAttribute = new CosmosCollectionAttribute("name");

            // Assert
            Assert.Equal(expectedName, collectionAttribute.Name);
        }
    }
}