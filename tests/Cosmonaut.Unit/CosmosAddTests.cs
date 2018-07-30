using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Response;
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
            var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);
            _mockDocumentClient.Setup(x => x.CreateDocumentAsync(It.IsAny<Uri>(),
                    It.IsAny<object>(), It.IsAny<RequestOptions>(), false))
                .ReturnsAsync(resourceResponse);
                
            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", "", "http://test.com");

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

                var document = dummy.ConvertObjectToDocument();
                var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);
                _mockDocumentClient.Setup(x => x.CreateDocumentAsync(It.IsAny<Uri>(),
                        It.IsAny<object>(), It.IsAny<RequestOptions>(), false))
                    .ReturnsAsync(resourceResponse);
            }

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", "", "http://test.com");

            // Act
            var result = await entityStore.AddRangeAsync(dummies);

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.FailedEntities.Should().BeEmpty();
            result.SuccessfulEntities.Should().HaveCount(5);
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

                var document = dummy.ConvertObjectToDocument();
                var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);
                _mockDocumentClient.Setup(x => x.CreateDocumentAsync(It.IsAny<Uri>(),
                        It.IsAny<object>(), It.IsAny<RequestOptions>(), false))
                    .ReturnsAsync(resourceResponse);
            }

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", "", "http://test.com");

            // Act
            var result = await entityStore.AddRangeAsync(dummies[0], dummies[1], dummies[2], dummies[3], dummies[4]);

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.FailedEntities.Should().BeEmpty();
            result.SuccessfulEntities.Should().HaveCount(5);
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
            var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);
            _mockDocumentClient.Setup(x => x.CreateDocumentAsync(It.IsAny<Uri>(),
                    It.IsAny<object>(), It.IsAny<RequestOptions>(), false))
                .ReturnsAsync(resourceResponse);

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", "", "http://test.com");

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