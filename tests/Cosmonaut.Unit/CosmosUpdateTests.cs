using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Testing;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
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
            var document = addedDummy.ConvertObjectToDocument();
            var resourceResponse = document.ToResourceResponse(HttpStatusCode.OK);
            document.SetPropertyValue(nameof(addedDummy.Name), expectedName);
            _mockDocumentClient.Setup(x => x.ReplaceDocumentAsync(It.IsAny<Uri>(), document.ItIsSameDocument(), It.IsAny<RequestOptions>(), CancellationToken.None)).ReturnsAsync(resourceResponse);

            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

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
            var resourceResponse = document.ToResourceResponse(HttpStatusCode.OK);
            document.SetPropertyValue(nameof(addedDummy.Name), "newTest");
            _mockDocumentClient.Setup(x => x.ReplaceDocumentAsync(It.IsAny<Uri>(), document.ItIsSameDocument(), It.IsAny<RequestOptions>(), CancellationToken.None))
                .ReturnsAsync(resourceResponse);
            
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");
            addedDummy.Name = "newTest";
            // Act
            var result = await entityStore.UpdateRangeAsync(new []{ addedDummy });

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
                Name = "Test"
            };

            var document = addedDummy.ConvertObjectToDocument();
            var resourceResponse = document.ToResourceResponse(HttpStatusCode.OK);
            document.SetPropertyValue(nameof(addedDummy.Name), "newTest");
            _mockDocumentClient.Setup(x => x.UpsertDocumentAsync(It.IsAny<Uri>(), document.ItIsSameDocument(), It.IsAny<RequestOptions>(), false, CancellationToken.None))
                .ReturnsAsync(resourceResponse);

            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");
            addedDummy.Name = "newTest";

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
            var resourceResponse = document.ToResourceResponse(HttpStatusCode.OK);
            document.SetPropertyValue(nameof(addedDummy.Name), "newTest");
            _mockDocumentClient.Setup(x => x.UpsertDocumentAsync(It.IsAny<Uri>(), document.ItIsSameDocument(), It.IsAny<RequestOptions>(), false, CancellationToken.None))
                .ReturnsAsync(resourceResponse);

            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");
            addedDummy.Name = "newTest";
            // Act
            var result = await entityStore.UpsertRangeAsync(new []{ addedDummy });

            // Assert
            result.FailedEntities.Should().BeEmpty();
            result.SuccessfulEntities.Should().HaveCount(1);
            result.SuccessfulEntities.Single().ResourceResponse.Resource.Should().BeEquivalentTo(document);
            result.SuccessfulEntities.Single().ResourceResponse.Resource.Should().NotBeNull();
        }
    }
}