using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Testing;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Cosmonaut.Unit
{
    public class CosmosStoreTests
    {
        [Fact]
        public void DatabaseAndCollectionReturnSelfLink()
        {
            // Arrange
            var databaseName = "dbName";
            var collectionName = "collName";
            var documentId = "documentId";
            var expectedLink = $"dbs/{databaseName}/colls/{collectionName}/docs/{documentId}";

            // Act
            var selfLink = UriFactory.CreateDocumentUri(databaseName,collectionName,documentId);

            // Assert
            selfLink.ToString().Should().Be(expectedLink);
        }

        [Fact]
        public void HasJsonPropertyAttributeIdReturnsTrueIfPresent()
        {
            // Arrange
            var dummy = new DummyWithIdAndWithAttr
            {
                ActualyId = Guid.NewGuid().ToString()
            };

            var porentialJsonPropertyAttribute = typeof(DummyWithIdAndWithAttr)
                .GetProperty(nameof(DummyWithIdAndWithAttr.ActualyId))
                .GetCustomAttribute<JsonPropertyAttribute>();

            // Act
            var result = porentialJsonPropertyAttribute.HasJsonPropertyAttributeId();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasJsonPropertyAttributeIdReturnsFalseIfMissing()
        {
            // Arrange
            var dummy = new DummyWithIdAndWithAttr
            {
                ActualyId = Guid.NewGuid().ToString()
            };

            var porentialJsonPropertyAttribute = typeof(DummyWithIdAndWithAttr)
                .GetProperty(nameof(DummyWithIdAndWithAttr.Id))
                .GetCustomAttribute<JsonPropertyAttribute>();

            // Act
            var result = porentialJsonPropertyAttribute.HasJsonPropertyAttributeId();

            // Assert
            result.Should().BeFalse();
        }
        
        [Fact]
        public async Task FindAsync_ReturnsEntity_WhenFoundInCosmosDB()
        {
            // Arrange
            Mock<IDocumentClient> mockDocumentClient = MockHelpers.GetMockDocumentClient();
            var id = Guid.NewGuid().ToString();
            var dummy = new Dummy
            {
                Id = id,
                Name = "Nick"
            };
            var document = dummy.ConvertObjectToDocument();
            var resourceResponse = document.ToResourceResponse(HttpStatusCode.OK);
            mockDocumentClient.Setup(x => x.ReadDocumentAsync(UriFactory.CreateDocumentUri("databaseName", "dummies", id), It.IsAny<RequestOptions>()))
                .ReturnsAsync(resourceResponse);

            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(() => mockDocumentClient.Object), "databaseName");

            // Act
            var result = await entityStore.FindAsync(id);

            //Assert
            result.Should().BeEquivalentTo(dummy);
        }
    }
}