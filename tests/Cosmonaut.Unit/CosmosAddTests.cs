using System;
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
            var document = dummy.ConvertObjectToDocument();
            var resourceResponse = document.ToResourceResponse(HttpStatusCode.OK);
            _mockDocumentClient.Setup(x => x.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("databaseName", "dummies"),
                    document.ItIsSameDocument(), It.IsAny<RequestOptions>(), false, CancellationToken.None))
                .ReturnsAsync(resourceResponse);
                
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

            // Act
            var result = await entityStore.AddAsync(dummy);
            
            //Assert
            result.IsSuccess.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(dummy);
            result.ResourceResponse.Resource.Should().NotBeNull();
            result.ResourceResponse.Resource.Should().BeEquivalentTo(document);
            result.CosmosOperationStatus.Should().Be(CosmosOperationStatus.Success);
            result.ResourceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AddEntityWithoutIdEmptyGeneratedId()
        {
            // Arrange
            var dummy = new Dummy
            {
                Name = "Nick"
            };
            var document = dummy.ConvertObjectToDocument();
            var resourceResponse = document.ToResourceResponse(HttpStatusCode.OK);
            _mockDocumentClient.Setup(x => x.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("databaseName", "dummies"),
                    document.ItIsSameDocument(), It.IsAny<RequestOptions>(), false, CancellationToken.None))
                .ReturnsAsync(resourceResponse);

            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

            // Act
            var result = await entityStore.AddAsync(dummy);

            //Assert
            var isGuid = Guid.TryParse(result.Entity.Id, out var guid);
            isGuid.Should().BeTrue();
            guid.Should().NotBeEmpty();
            result.IsSuccess.Should().BeTrue();
            result.ResourceResponse.Resource.Should().NotBeNull();
            result.ResourceResponse.Resource.Should().BeEquivalentTo(document);
            result.Entity.Should().BeEquivalentTo(dummy);
            result.CosmosOperationStatus.Should().Be(CosmosOperationStatus.Success);
            result.ResourceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}