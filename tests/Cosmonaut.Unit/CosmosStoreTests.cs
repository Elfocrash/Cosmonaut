using System;
using System.Reflection;
using Cosmonaut.Extensions;
using FluentAssertions;
using Microsoft.Azure.Documents.Client;
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
    }
}