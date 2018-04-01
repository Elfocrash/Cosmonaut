using System;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosStoreSettingsTests
    {
        [Fact]
        public void InvalidEnpointThrowsExpection()
        {
            // Arrange
            var action = new Action(()=> new CosmosStoreSettings("dbName", "invalidEndpoint", "key"));

            // Act &  Assert
            Assert.Throws<UriFormatException>(action);
        }

        [Fact]
        public void ValidStringEndpointCreatesUri()
        {
            // Arrange
            var expectedUri = new Uri("http://test.com");

            // Act
            var settings = new CosmosStoreSettings("dbName", "http://test.com", "key");

            // Assert
            Assert.Equal(expectedUri, settings.EndpointUrl);
        }
    }
}