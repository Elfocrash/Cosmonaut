using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Cosmonaut.Unit
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
            var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);
            _mockDocumentClient.Setup(x => x.ReplaceDocumentAsync(It.IsAny<Uri>(), It.IsAny<Document>(), It.IsAny<RequestOptions>())).ReturnsAsync(resourceResponse);

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", "", "http://test.com");

            // Act
            addedDummy.Name = expectedName;
            var result = await entityStore.UpdateAsync(addedDummy);

            // Assert
            result.ResourceResponse.Resource.Should().NotBeNull();
            result.ResourceResponse.Resource.Should().BeEquivalentTo(document);
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
            JToken jtoken = JToken.FromObject(document);
            var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);

            _mockDocumentClient.Setup(x => x.ReplaceDocumentAsync(It.IsAny<Uri>(), It.IsAny<Document>(), It.IsAny<RequestOptions>()))
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
            JObject jtoken = JObject.FromObject(document);
            var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);

            _mockDocumentClient.Setup(x => x.UpsertDocumentAsync(It.IsAny<Uri>(), It.IsAny<Document>(), It.IsAny<RequestOptions>(), false))
                .ReturnsAsync(resourceResponse);

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", "", "http://test.com");

            // Act
            var result = await entityStore.UpsertAsync(addedDummy);

            // Assert
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
            JObject jtoken = JObject.FromObject(document);
            var resourceResponse = MockHelpers.CreateResourceResponse(document, HttpStatusCode.OK);

            _mockDocumentClient.Setup(x => x.UpsertDocumentAsync(It.IsAny<Uri>(), It.IsAny<Document>(), It.IsAny<RequestOptions>(), false))
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