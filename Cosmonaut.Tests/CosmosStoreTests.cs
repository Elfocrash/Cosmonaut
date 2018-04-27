using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Storage;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
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
            var expectedSelfLink = $"dbs/{databaseName}/colls/{collectionName}/docs/{documentId}/";

            // Act
            var selfLink = DocumentHelpers.GetDocumentSelfLink(databaseName,collectionName,documentId);

            // Assert
            Assert.Equal(expectedSelfLink, selfLink);
        }
    }
}