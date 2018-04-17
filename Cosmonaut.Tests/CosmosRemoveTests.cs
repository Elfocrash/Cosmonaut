using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Response;
using Cosmonaut.Storage;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosRemoveTests
    {
        private readonly Mock<IDocumentClient> _mockDocumentClient;
        private readonly ICosmosStore<Dummy> _dummyStore;

        public CosmosRemoveTests()
        {
            _mockDocumentClient = MockHelpers.GetFakeDocumentClient();
            _dummyStore = new InMemoryCosmosStore<Dummy>();
        }

        [Fact]
        public async Task RemoveEntityRemoves()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var addedDummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };

            _mockDocumentClient.Setup(x => x.DeleteDocumentAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new ResourceResponse<Document>(new Document { Id = id }));
            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator(_mockDocumentClient.Object));

            // Act
            var result = await entityStore.RemoveAsync(addedDummy);

            // Assert
            Assert.Equal(CosmosOperationStatus.Success, result.CosmosOperationStatus);
        }

        [Fact]
        public async Task RemoveByIdRemoves()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var addedDummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };
            var response = new ResourceResponse<Document>(new Document { Id = addedDummy.Id });
            _mockDocumentClient.Setup(x => x.DeleteDocumentAsync(It.IsAny<string>(), null))
                .ReturnsAsync(response);
            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator(_mockDocumentClient.Object));

            // Act
            var result = await entityStore.RemoveByIdAsync(id);

            // Assert
            Assert.Equal(CosmosOperationStatus.Success, result.CosmosOperationStatus);
        }

        [Fact]
        public async Task RemoveByExpressionRemoves()
        {
            // Arrange
            foreach (var i in Enumerable.Range(0, 10))
            {
                var id = Guid.NewGuid().ToString();
                var addedDummy = new Dummy
                {
                    Id = id,
                    Name = "Test " + i
                };
                await _dummyStore.AddAsync(addedDummy);
            }

            // Act
            var result = await _dummyStore.RemoveAsync(x => x.Name.Contains("Test"));

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.FailedEntities);
        }

        [Fact]
        public async Task RemoveRangeRemoves()
        {
            // Arrange
            var addedList = new List<Dummy>();
            foreach (var i in Enumerable.Range(0, 10))
            {
                var id = Guid.NewGuid().ToString();
                var addedDummy = new Dummy
                {
                    Id = id,
                    Name = "Test " + i
                };
                await _dummyStore.AddAsync(addedDummy);
                addedList.Add(addedDummy);
            }

            // Act
            var result = await _dummyStore.RemoveRangeAsync(addedList);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.FailedEntities);
        }
    }
}