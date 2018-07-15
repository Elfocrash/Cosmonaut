using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Storage;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosUpdateTests
    {
        private readonly Mock<IDocumentClient> _mockDocumentClient;

        public CosmosUpdateTests()
        {
            _mockDocumentClient = MockHelpers.GetMockDocumentClient();
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
            addedDummy.ValidateEntityForCosmosDb();
            var documentId = addedDummy.GetDocumentId();
            var document = addedDummy.ConvertObjectToDocument();
            JToken jtoken = JToken.FromObject(document);
            _mockDocumentClient.Setup(x => x.ReplaceDocumentAsync(It.IsAny<Uri>(), jtoken, It.IsAny<RequestOptions>())).ReturnsAsync(new ResourceResponse<Document>(document));

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", "", "http://test.com");

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
            addedDummy.ValidateEntityForCosmosDb();
            var document = addedDummy.ConvertObjectToDocument();
            var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);

            _mockDocumentClient.Setup(x => x.ReplaceDocumentAsync(It.IsAny<Uri>(), It.IsAny<object>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(resourceResponse);
            
            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", "", "http://test.com");
            addedDummy.Name = "newTest";
            // Act
            var result = await entityStore.UpdateRangeAsync(addedDummy);

            // Assert
            result.FailedEntities.Should().BeEmpty();
            result.SuccessfulEntities.Should().HaveCount(1);
            result.SuccessfulEntities.Single().ResourceResponse.Resource.Should().NotBeNull();
            result.SuccessfulEntities.Single().ResourceResponse.Resource.Should().BeEquivalentTo(document);
        }
        
        [Fact]
        public async Task UpsertEntityUpsert()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var addedDummy = new Dummy
            {
                Id = id,
                Name = "NewTest"
            };

            var document = addedDummy.ConvertObjectToDocument();
            var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);

            _mockDocumentClient.Setup(x => x.UpsertDocumentAsync(It.IsAny<Uri>(), It.IsAny<object>(), It.IsAny<RequestOptions>(), false))
                .ReturnsAsync(resourceResponse);

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", "", "http://test.com");

            // Act
            var result = await entityStore.UpsertAsync(addedDummy);

            // Assert
            result.ResourceResponse.Should().BeEquivalentTo(resourceResponse);
            result.ResourceResponse.Resource.Should().NotBeNull();
            result.ResourceResponse.Resource.Should().BeEquivalentTo(document);
        }

        [Fact]
        public async Task UpsertRangeUpsertsEntities()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var addedDummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };
            var document = addedDummy.ConvertObjectToDocument();
            var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);

            _mockDocumentClient.Setup(x => x.UpsertDocumentAsync(It.IsAny<Uri>(), It.IsAny<object>(), It.IsAny<RequestOptions>(), false))
                .ReturnsAsync(resourceResponse);

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", "", "http://test.com");
            addedDummy.Name = "newTest";
            // Act
            var result = await entityStore.UpsertRangeAsync(addedDummy);

            // Assert
            result.FailedEntities.Should().BeEmpty();
            result.SuccessfulEntities.Should().HaveCount(1);
            result.SuccessfulEntities.Single().ResourceResponse.Resource.Should().BeEquivalentTo(document);
            result.SuccessfulEntities.Single().ResourceResponse.Resource.Should().NotBeNull();
            result.SuccessfulEntities.Single().ResourceResponse.Resource.Should().BeEquivalentTo(document);
        }
    }
}