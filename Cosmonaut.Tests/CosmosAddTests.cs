using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cosmonaut.Exceptions;
using Cosmonaut.Response;
using Cosmonaut.Storage;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosAddTests
    {
        private readonly Mock<IDocumentClient> _mockDocumentClient;
        private readonly CosmosDocumentProcessor<Dummy> _documentProcessor;
        public CosmosAddTests()
        {
            _mockDocumentClient = MockHelpers.GetFakeDocumentClient();
            _documentProcessor = new CosmosDocumentProcessor<Dummy>();
        }

        [Fact]
        public async Task AddValidObjectSuccess()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummy = new Dummy
            {
                Id = id,
                Name = "Nick"
            };
            _mockDocumentClient.Setup(x => x.CreateDocumentAsync(It.IsAny<string>(),
                _documentProcessor.GetCosmosDbFriendlyEntity(dummy) as Document, null, false))
                .ReturnsAsync(new ResourceResponse<Document>());
                
            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator<Dummy>(_mockDocumentClient.Object, new CosmosDocumentProcessor<Dummy>()));

            // Act
            var expectedResponse = new CosmosResponse<Dummy>(dummy, CosmosOperationStatus.Success);
            var result = await entityStore.AddAsync(dummy);
            
            //Assert
            Assert.Equal(expectedResponse.Entity, result.Entity);
            Assert.Equal(expectedResponse.IsSuccess, result.IsSuccess);
        }

        [Fact]
        public async Task AddEntityWithoutIdEmptyGeneratedId()
        {
            // Arrange
            var dummy = new Dummy
            {
                Name = "Nick"
            };
            _mockDocumentClient.Setup(x => x.CreateDocumentAsync(It.IsAny<string>(),
                    _documentProcessor.GetCosmosDbFriendlyEntity(dummy) as Document, null, false))
                .ReturnsAsync(new ResourceResponse<Document>());

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator<Dummy>(_mockDocumentClient.Object, new CosmosDocumentProcessor<Dummy>()));

            // Act
            var result = await entityStore.AddAsync(dummy);

            //Assert
            var isGuid = Guid.TryParse(result.Entity.Id, out var guid);
            Assert.True(isGuid);
            Assert.NotEqual(Guid.Empty, guid);
        }
    }
}