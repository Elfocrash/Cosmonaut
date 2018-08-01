using System;
using FluentAssertions;
using Xunit;

namespace Cosmonaut.Unit
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

        [Fact]
        public void CosmosStoreSettings_Defaults_CreatedCorrectDefaults()
        {
            // Arrange
            var endpointUri = new Uri("http://test.com");

            // Act
            var settings = new CosmosStoreSettings("dbName", endpointUri, "key");

            // Assert
            settings.EndpointUrl.Should().Be(endpointUri);
            settings.AuthKey.Should().Be("key");
            settings.ConnectionPolicy.Should().BeNull();
            settings.ConsistencyLevel.Should().BeNull();
            settings.IndexingPolicy.Should().BeNull();
            settings.DefaultCollectionThroughput.Should().Be(CosmosConstants.MinimumCosmosThroughput);
            settings.MaximumUpscaleRequestUnits.Should().Be(CosmosConstants.DefaultMaximumUpscaleThroughput);
        }

        [Fact]
        public void CosmosStoreSettings_ParameterlessCtor_CreatedCorrectDefaults()
        {
            // Arrange
            var settings = new CosmosStoreSettings();
            
            // Act & Assert
            settings.EndpointUrl.Should().BeNull();
            settings.AuthKey.Should().BeNull();
            settings.ConnectionPolicy.Should().BeNull();
            settings.ConsistencyLevel.Should().BeNull();
            settings.IndexingPolicy.Should().BeNull();
            settings.DefaultCollectionThroughput.Should().Be(CosmosConstants.MinimumCosmosThroughput);
            settings.MaximumUpscaleRequestUnits.Should().Be(CosmosConstants.DefaultMaximumUpscaleThroughput);
        }
    }
}