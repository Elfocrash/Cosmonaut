using System.Net;
using Cosmonaut.Storage;
using Microsoft.Azure.Documents;
using Moq;
using Xunit;

namespace Cosmonaut.Tests
{
    public class ScalingTests
    {
        private readonly Mock<IDocumentClient> _mockDocumentClient;

        public ScalingTests()
        {
            _mockDocumentClient = MockHelpers.GetMockDocumentClient();
        }

        [Fact]
        public void AdjustCollectionThroughputSetsThroughput()
        {
            // Arrange
            var newOffer = new Offer();
            var resource = MockHelpers.CreateResourceResponse<Offer>(newOffer, HttpStatusCode.OK);
            _mockDocumentClient.Setup(x => x.ReplaceOfferAsync(It.IsAny<Offer>())).ReturnsAsync(resource);

            // Act
            var entityStore = new CosmosStore<DummyWithThroughput>(
                _mockDocumentClient.Object,
                "databaseName",
                new CosmosDatabaseCreator(_mockDocumentClient.Object),
                new CosmosCollectionCreator(_mockDocumentClient.Object), true, true, true);
            
            // Assert
            Assert.Equal(500, entityStore.CollectionThrouput);
        }
    }
}