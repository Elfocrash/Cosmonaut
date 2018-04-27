using System;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Storage;
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
            var document = addedDummy.GetCosmosDbFriendlyEntity();
            JsonReader reader = new JTokenReader(document);
            var actualDocument = new Document();
            actualDocument.LoadFrom(reader);
            JToken jtoken = JToken.FromObject(document);
            _mockDocumentClient.Setup(x => x.ReplaceDocumentAsync(DocumentHelpers.GetDocumentSelfLink("databaseName", It.IsAny<string>(), documentId), jtoken, It.IsAny<RequestOptions>())).ReturnsAsync(new ResourceResponse<Document>(actualDocument));

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator(_mockDocumentClient.Object));

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
            var documentId = addedDummy.GetDocumentId();
            var document = addedDummy.GetCosmosDbFriendlyEntity();
            JsonReader reader = new JTokenReader(document);
            var actualDocument = new Document();
            actualDocument.LoadFrom(reader);
            JToken jtoken = JToken.FromObject(document);
            _mockDocumentClient.Setup(x => x.ReplaceDocumentAsync(DocumentHelpers.GetDocumentSelfLink("databaseName", It.IsAny<string>(), documentId), jtoken, It.IsAny<RequestOptions>())).ReturnsAsync(new ResourceResponse<Document>(actualDocument));
            
            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator(_mockDocumentClient.Object));
            addedDummy.Name = "newTest";
            // Act
            var result = await entityStore.UpdateRangeAsync(addedDummy);

            // Assert
            Assert.Empty(result.FailedEntities);
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

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator(_mockDocumentClient.Object));

            // Act
            var result = await entityStore.UpsertAsync(addedDummy);

            // Assert
            Assert.Equal(expectedName, result.ResourceResponse.Resource.GetPropertyValue<string>("Name"));
        }
    }
}