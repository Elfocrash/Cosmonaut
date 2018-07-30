using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Storage;
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
            var expectedDatabase = new Database {Id = databaseName};
            var orderQueriable = new EnumerableQuery<Database>(new List<Database> {expectedDatabase});

            var mockDocumentClient = new Mock<IDocumentClient>();
            mockDocumentClient.Setup(x => x.CreateDatabaseQuery(null)).Returns(orderQueriable);
            var databaseCreator = new CosmosDatabaseCreator(mockDocumentClient.Object);

            // Act
            var created = await databaseCreator.EnsureCreatedAsync(databaseName);

            // Assert
            Assert.False(created);
        }

        [Fact]
        public async Task CreationOfNotExistingDatabaseReturnTrue()
        {
            // Arrange
            var databaseName = "test";
            var expectedDatabase = new Database { Id = databaseName };
            var orderQueriable = new EnumerableQuery<Database>(new List<Database>());

            var mockDocumentClient = new Mock<IDocumentClient>();
            mockDocumentClient.Setup(x => x.CreateDatabaseQuery(null)).Returns(orderQueriable);
            mockDocumentClient.Setup(x => x.CreateDatabaseAsync(It.IsAny<Database>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(new ResourceResponse<Database>(expectedDatabase));
            var databaseCreator = new CosmosDatabaseCreator(mockDocumentClient.Object);

            // Act
            var created = await databaseCreator.EnsureCreatedAsync(databaseName);

            // Assert
            Assert.True(created);
        }
    }
}