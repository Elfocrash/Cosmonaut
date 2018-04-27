using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Response;
using Cosmonaut.Storage;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosAddTests
    {
        private readonly Mock<IDocumentClient> _mockDocumentClient;

        public CosmosAddTests()
        {
            _mockDocumentClient = MockHelpers.GetMockDocumentClient();
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
            var document = dummy.GetCosmosDbFriendlyEntity() as Document;
            var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);
            _mockDocumentClient.Setup(x => x.CreateDocumentAsync(It.IsAny<string>(),
                    It.IsAny<object>(), It.IsAny<RequestOptions>(), false))
                .ReturnsAsync(resourceResponse);
                
            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator(_mockDocumentClient.Object));

            // Act
            var result = await entityStore.AddAsync(dummy);
            
            //Assert
            result.IsSuccess.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(dummy);
            result.CosmosOperationStatus.Should().Be(CosmosOperationStatus.Success);
            result.ResourceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AddRangeValidObjectsSuccess()
        {
            // Arrange
            var dummies = new List<Dummy>();
            for (int i = 0; i < 5; i++)
            {
                var id = i.ToString();
                var dummy = new Dummy
                {
                    Id = id,
                    Name = "Nick"
                };
                dummies.Add(dummy);

                var document = dummy.GetCosmosDbFriendlyEntity() as Document;
                var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);
                _mockDocumentClient.Setup(x => x.CreateDocumentAsync(It.IsAny<string>(),
                        It.IsAny<object>(), It.IsAny<RequestOptions>(), false))
                    .ReturnsAsync(resourceResponse);
            }

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator(_mockDocumentClient.Object));

            // Act
            var result = await entityStore.AddRangeAsync(dummies);

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.FailedEntities.Should().BeEmpty();
        }

        [Fact]
        public async Task AddRangeParamsValidObjectsSuccess()
        {
            // Arrange
            var dummies = new List<Dummy>();
            for (int i = 0; i < 5; i++)
            {
                var id = i.ToString();
                var dummy = new Dummy
                {
                    Id = id,
                    Name = "Nick"
                };
                dummies.Add(dummy);

                var document = dummy.GetCosmosDbFriendlyEntity() as Document;
                var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);
                _mockDocumentClient.Setup(x => x.CreateDocumentAsync(It.IsAny<string>(),
                        It.IsAny<object>(), It.IsAny<RequestOptions>(), false))
                    .ReturnsAsync(resourceResponse);
            }

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator(_mockDocumentClient.Object));

            // Act
            var result = await entityStore.AddRangeAsync(dummies[0], dummies[1]);

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.FailedEntities.Should().BeEmpty();
        }

        [Fact]
        public async Task AddEntityWithoutIdEmptyGeneratedId()
        {
            // Arrange
            var dummy = new Dummy
            {
                Name = "Nick"
            };
            var document = dummy.GetCosmosDbFriendlyEntity() as Document;
            var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);
            _mockDocumentClient.Setup(x => x.CreateDocumentAsync(It.IsAny<string>(),
                    It.IsAny<object>(), It.IsAny<RequestOptions>(), false))
                .ReturnsAsync(resourceResponse);

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator(_mockDocumentClient.Object));

            // Act
            var result = await entityStore.AddAsync(dummy);

            //Assert
            var isGuid = Guid.TryParse(result.Entity.Id, out var guid);
            isGuid.Should().BeTrue();
            guid.Should().NotBeEmpty();
            result.IsSuccess.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(dummy);
            result.CosmosOperationStatus.Should().Be(CosmosOperationStatus.Success);
            result.ResourceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}