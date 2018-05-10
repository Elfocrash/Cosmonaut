using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Storage;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Cosmonaut.Tests
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
        public void RemovePotentialDuplicateIdPropertiesRemovesNoMatterTheCase()
        {
            // Arrange
            var dynamicObject = new
            {
                iD = "1",
                Id = "2",
                ID = "3"
            };

            dynamic obj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(dynamicObject));

            // Act
            DocumentEntityExtensions.RemovePotentialDuplicateIdProperties(ref obj);
            
            // Assert
            ((string)obj.iD?.ToString()).Should().BeNull();
            ((string)obj.ID?.ToString()).Should().BeNull();
            ((string)obj.Id?.ToString()).Should().BeNull();
        }
    }
}