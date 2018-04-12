using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Response;
using Cosmonaut.Storage;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosUpdateTests
    {
        private readonly Mock<IDocumentClient> _mockDocumentClient;
        private readonly CosmosDocumentProcessor<Dummy> _documentProcessor;

        public CosmosUpdateTests()
        {
            _mockDocumentClient = MockHelpers.GetFakeDocumentClient();
            _documentProcessor = new CosmosDocumentProcessor<Dummy>();
        }

        [Fact]
        public async Task UpdateEntityUpdates()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var addedDummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };
            var expectedName = "NewTest";
            _mockDocumentClient.Setup(x => x.CreateDocumentQuery<Document>(It.IsAny<string>(), It.IsAny<FeedOptions>()))
                .Returns(new EnumerableQuery<Document>(new List<Document> { new Document { Id = id } }));

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator<Dummy>(_mockDocumentClient.Object, new CosmosDocumentProcessor<Dummy>()));

            // Act
            addedDummy.Name = expectedName;
            var result = await entityStore.UpdateAsync(addedDummy);

            // Assert
            Assert.Equal(expectedName, result.Entity.Name);
        }

        [Fact]
        public async Task UpdateRangeUpdatesEntities()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var addedDummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };
            _mockDocumentClient.Setup(x => x.CreateDocumentQuery<Document>(It.IsAny<string>(), It.IsAny<FeedOptions>()))
                .Returns(new EnumerableQuery<Document>(new List<Document> { new Document { Id = id } }));

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator<Dummy>(_mockDocumentClient.Object, new CosmosDocumentProcessor<Dummy>()));
            addedDummy.Name = "newTest";
            // Act
            var result = await entityStore.UpdateRangeAsync(addedDummy);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.FailedEntities);
        }
        
        [Fact]
        public async Task UpdateEntityThatHasIdChangedFails()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var addedDummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };
            _mockDocumentClient.Setup(x => x.CreateDocumentQuery<Document>(It.IsAny<string>(), It.IsAny<FeedOptions>()))
                .Returns(new EnumerableQuery<Document>(new List<Document>{new Document{Id = id}}));

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator<Dummy>(_mockDocumentClient.Object, new CosmosDocumentProcessor<Dummy>()));

            // Act
            addedDummy.Id = Guid.NewGuid().ToString();
            var result = await entityStore.UpdateAsync(addedDummy);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(addedDummy, result.Entity);
            Assert.Equal(CosmosOperationStatus.ResourceNotFound, result.CosmosOperationStatus);
        }

        [Fact]
        public async Task UpsertEntityUpsert()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var addedDummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };
            var expectedName = "NewTest";
            var expectedDocument = new Document
            {
                Id = id
            };
            expectedDocument.SetPropertyValue("Name", expectedName);
            _mockDocumentClient.Setup(x => x.UpsertDocumentAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<RequestOptions>(), false))
                .ReturnsAsync(new ResourceResponse<Document>(expectedDocument));

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator<Dummy>(_mockDocumentClient.Object, new CosmosDocumentProcessor<Dummy>()));

            // Act
            var result = await entityStore.UpsertAsync(addedDummy);

            // Assert
            Assert.Equal(expectedName, result.ResourceResponse.Resource.GetPropertyValue<string>("Name"));
        }
    }
}