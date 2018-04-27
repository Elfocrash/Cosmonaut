using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Storage;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosQueryTests
    {
        private readonly Mock<IDocumentClient> _mockDocumentClient;

        public CosmosQueryTests()
        {
            _mockDocumentClient = MockHelpers.GetMockDocumentClient();
        }
        
        [Fact]
        public async Task ToListAsyncReturnsList()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummy = new Dummy
            {
                Id = id,
                Name = "Nick"
            };
            Expression<Func<Dummy, bool>> predicate = t => true;

            var list = new List<Dummy> {dummy};
            var dataSource = list.AsQueryable();
            var expected = dataSource.Where(predicate);

            var response = new FeedResponse<Dummy>(expected);

            var mockDocumentQuery = new Mock<IFakeDocumentQuery<Dummy>>();
            mockDocumentQuery
                .SetupSequence(_ => _.HasMoreResults)
                .Returns(true)
                .Returns(false);

            mockDocumentQuery
                .Setup(_ => _.ExecuteNextAsync<Dummy>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var provider = new Mock<IQueryProvider>();
            provider
                .Setup(_ => _.CreateQuery<Dummy>(It.IsAny<Expression>()))
                .Returns(mockDocumentQuery.Object);
            mockDocumentQuery.As<IQueryable<Dummy>>().Setup(x => x.Provider).Returns(provider.Object);
            mockDocumentQuery.As<IQueryable<Dummy>>().Setup(x => x.Expression).Returns(dataSource.Expression);
            mockDocumentQuery.As<IQueryable<Dummy>>().Setup(x => x.ElementType).Returns(dataSource.ElementType);
            mockDocumentQuery.As<IQueryable<Dummy>>().Setup(x => x.GetEnumerator()).Returns(() => dataSource.GetEnumerator());


            _mockDocumentClient.Setup(x => x.CreateDocumentQuery<Dummy>(It.IsAny<string>(),
                    It.IsAny<FeedOptions>()))
                .Returns(mockDocumentQuery.Object);

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName", new CosmosDatabaseCreator(_mockDocumentClient.Object), new CosmosCollectionCreator(_mockDocumentClient.Object));

            // Act
            var result = await entityStore.ToListAsync(predicate);

            //Assert
            result.Count.Should().Be(1);
            result.Should().BeEquivalentTo(list);
        }
    }
}