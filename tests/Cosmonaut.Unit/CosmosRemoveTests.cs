using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Response;
using Cosmonaut.Testing;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Xunit;

namespace Cosmonaut.Unit
{
    public class CosmosRemoveTests
    {
        private readonly Mock<IDocumentClient> _mockDocumentClient;

        public CosmosRemoveTests()
        {
            _mockDocumentClient = MockHelpers.GetMockDocumentClient();
        }

        [Fact]
        public async Task RemoveEntityRemoves()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };

            var document = dummy.ToCosmonautDocument(null);
            var resourceResponse = document.ToResourceResponse(HttpStatusCode.OK);

            _mockDocumentClient.Setup(x => x.DeleteDocumentAsync(It.IsAny<Uri>(), It.IsAny<RequestOptions>(), CancellationToken.None))
                .ReturnsAsync(resourceResponse);
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

            // Act
            var result = await entityStore.RemoveAsync(dummy);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(dummy);
            result.ResourceResponse.Resource.Should().NotBeNull();
            result.ResourceResponse.Resource.Should().BeEquivalentTo(document);
            result.CosmosOperationStatus.Should().Be(CosmosOperationStatus.Success);
            result.ResourceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RemoveByIdRemoves()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var toRemove = new Dummy
            {
                Id = id,
                Name = "Test"
            };
            var document = toRemove.ToCosmonautDocument(null);
            var resourceResponse = document.ToResourceResponse(HttpStatusCode.OK);
            _mockDocumentClient.Setup(x => x.DeleteDocumentAsync(It.IsAny<Uri>(), It.IsAny<RequestOptions>(), CancellationToken.None))
                .ReturnsAsync(resourceResponse);
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

            // Act
            var result = await entityStore.RemoveByIdAsync(id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ResourceResponse.Resource.Should().NotBeNull();
            result.ResourceResponse.Resource.Should().BeEquivalentTo(document);
            result.CosmosOperationStatus.Should().Be(CosmosOperationStatus.Success);
            result.ResourceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RemoveEntitiesRemoves()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };

            var dummies = new List<Dummy> {dummy};

            var document = dummy.ToCosmonautDocument(null);
            var resourceResponse = document.ToResourceResponse(HttpStatusCode.OK);
            _mockDocumentClient.Setup(x => x.DeleteDocumentAsync(It.IsAny<Uri>(), It.IsAny<RequestOptions>(), CancellationToken.None))
                .ReturnsAsync(resourceResponse);
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

            // Act
            var result = await entityStore.RemoveRangeAsync(dummies);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.FailedEntities.Should().BeEmpty();
            result.SuccessfulEntities.Should().HaveCount(1);
            result.SuccessfulEntities.Single().ResourceResponse.Resource.Should().NotBeNull();
            result.SuccessfulEntities.Single().ResourceResponse.Resource.Should().BeEquivalentTo(document);
        }
    }
}