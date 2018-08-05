using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Storage;
using Cosmonaut.Testing;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Xunit;

namespace Cosmonaut.Unit
{
    public class DatabaseCreatorTests
    {
        [Fact]
        public async Task CreationOfExistingDatabaseReturnsFalse()
        {
            // Arrange
            var databaseName = "test";
            var expectedDatabase = new Database {Id = databaseName}.ToResourceResponse(HttpStatusCode.OK);

            var mockDocumentClient = new Mock<IDocumentClient>();
            mockDocumentClient.Setup(x => x.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName), It.IsAny<RequestOptions>()))
                .ReturnsAsync(expectedDatabase);
            var databaseCreator = new CosmosDatabaseCreator(mockDocumentClient.Object);
            var databaseCreatorWithCosmonaut = new CosmosDatabaseCreator(new CosmonautClient(mockDocumentClient.Object));

            // Act
            var created = await databaseCreator.EnsureCreatedAsync(databaseName);
            var createdCosmonaut = await databaseCreatorWithCosmonaut.EnsureCreatedAsync(databaseName);

            // Assert
            created.Should().BeFalse();
            createdCosmonaut.Should().BeFalse();
        }

        [Fact]
        public async Task CreationOfNotExistingDatabaseReturnTrue()
        {
            // Arrange
            var databaseName = "test";
            var expectedDatabase = new Database { Id = databaseName };
            var mockDocumentClient = new Mock<IDocumentClient>();
            mockDocumentClient.Setup(x => x.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName), It.IsAny<RequestOptions>()))
                .ReturnsAsync(((Database)null).ToResourceResponse(HttpStatusCode.NotFound));
            mockDocumentClient.Setup(x => x.CreateDatabaseAsync(It.IsAny<Database>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(expectedDatabase.ToResourceResponse(HttpStatusCode.OK));
            var databaseCreator = new CosmosDatabaseCreator(mockDocumentClient.Object);
            var databaseCreatorCosmonaut = new CosmosDatabaseCreator(new CosmonautClient(mockDocumentClient.Object));

            // Act
            var created = await databaseCreator.EnsureCreatedAsync(databaseName);
            var createdCosmonaut = await databaseCreatorCosmonaut.EnsureCreatedAsync(databaseName);

            // Assert
            created.Should().BeTrue();
            createdCosmonaut.Should().BeTrue();
        }
    }
}