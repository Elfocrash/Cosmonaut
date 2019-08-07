using System;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
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
            settings.IndexingPolicy.Should().Be(CosmosConstants.DefaultIndexingPolicy);
            settings.DefaultCollectionThroughput.Should().Be(CosmosConstants.MinimumCosmosThroughput);
            settings.UniqueKeyPolicy.Should().Be(CosmosConstants.DefaultUniqueKeyPolicy);
        }

        [Fact]
        public void CosmosStoreSettings_DefaultCtor_CreatedCorrectDefaults()
        {
            // Arrange
            var settings = new CosmosStoreSettings("test", "http://test.com", "key");
            
            // Act & Assert
            settings.EndpointUrl.Should().BeEquivalentTo(new Uri("http://test.com"));
            settings.AuthKey.Should().BeEquivalentTo("key");
            settings.DatabaseName.Should().BeEquivalentTo("test");
            settings.ConnectionPolicy.Should().BeNull();
            settings.ConsistencyLevel.Should().BeNull();
            settings.IndexingPolicy.Should().Be(CosmosConstants.DefaultIndexingPolicy);
            settings.DefaultCollectionThroughput.Should().Be(CosmosConstants.MinimumCosmosThroughput);
            settings.UniqueKeyPolicy.Should().Be(CosmosConstants.DefaultUniqueKeyPolicy);
        }

        [Fact]
        public void CosmosStoreSettings_DefaultCtorWithAction_CreatedCorrectDefaults()
        {
            // Arrange
            var settings = new CosmosStoreSettings("test", "http://test.com", "key", setting =>
            {
                setting.DefaultCollectionThroughput = 5000;
                setting.IndexingPolicy = new IndexingPolicy();
                setting.ConsistencyLevel = ConsistencyLevel.Eventual;
                setting.ConnectionPolicy = ConnectionPolicy.Default;
                setting.UniqueKeyPolicy = new UniqueKeyPolicy();
            });

            // Act & Assert
            settings.EndpointUrl.Should().BeEquivalentTo(new Uri("http://test.com"));
            settings.AuthKey.Should().BeEquivalentTo("key");
            settings.DatabaseName.Should().BeEquivalentTo("test");
            settings.ConnectionPolicy.Should().BeEquivalentTo(ConnectionPolicy.Default);
            settings.ConsistencyLevel.Should().BeEquivalentTo(ConsistencyLevel.Eventual);
            settings.IndexingPolicy.Should().NotBeNull();
            settings.DefaultCollectionThroughput.Should().Be(5000);
            settings.UniqueKeyPolicy.Should().NotBeNull();
        }
    }
}