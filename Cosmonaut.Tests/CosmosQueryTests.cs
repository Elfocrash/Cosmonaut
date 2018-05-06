using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
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
            var dummies = new List<Dummy>
            {
                new Dummy
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Nick"
                },
                new Dummy
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "John"
                }
            };

            Expression<Func<Dummy, bool>> predicate = t => true;

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Where(predicate);

            var entityStore = ResponseSetup(expected, dataSource);

            // Act
            var result = await entityStore.Query().Where(predicate).ToListAsync();

            //Assert
            result.Count.Should().Be(2);
            result.Should().BeEquivalentTo(dummies);
        }

        [Fact]
        public async Task FirstOrDefaultAsyncReturnsObject()
        {
            // Arrange
            var firstDummy = new Dummy
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Nick"
            };

            var dummies = new List<Dummy>
            {
                firstDummy,
                new Dummy
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "John"
                }
            };
            Expression<Func<Dummy, bool>> predicate = t => true;

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Where(predicate);

            var entityStore = ResponseSetup(expected, dataSource);

            // Act
            var result = await entityStore.Query().FirstOrDefaultAsync(predicate);

            //Assert
            result.Should().BeEquivalentTo(firstDummy);
        }

        [Fact]
        public async Task FirstAsyncReturnsObject()
        {
            // Arrange
            var firstDummy = new Dummy
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Nick"
            };

            var dummies = new List<Dummy>
            {
                firstDummy,
                new Dummy
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "John"
                }
            };
            Expression<Func<Dummy, bool>> predicate = t => true;

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Where(predicate);

            var entityStore = ResponseSetup(expected, dataSource);

            // Act
            var result = await entityStore.Query().FirstAsync(predicate);

            //Assert
            result.Should().BeEquivalentTo(firstDummy);
        }

        [Fact]
        public async Task SingleOrDefaultAsyncReturnsObject()
        {
            // Arrange
            var singleDummy = new Dummy
            {
                Id = Guid.NewGuid().ToString(),
                Name = "John"
            };

            var dummies = new List<Dummy>
            {
                new Dummy
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Nick"
                },
                singleDummy
            };
            Expression<Func<Dummy, bool>> predicate = x => x.Name == "John";

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Where(predicate);

            var entityStore = ResponseSetup(expected, dataSource);

            // Act
            var result = await entityStore.Query().SingleOrDefaultAsync(predicate);

            //Assert
            result.Should().BeEquivalentTo(singleDummy);
        }

        [Fact]
        public async Task SingleAsyncReturnsObject()
        {
            // Arrange
            var singleDummy = new Dummy
            {
                Id = Guid.NewGuid().ToString(),
                Name = "John"
            };

            var dummies = new List<Dummy>
            {
                new Dummy
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Nick"
                },
                singleDummy
            };
            Expression<Func<Dummy, bool>> predicate = x => x.Name == "John";

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Where(predicate);

            var entityStore = ResponseSetup(expected, dataSource);

            // Act
            var result = await entityStore.Query().SingleAsync(predicate);

            //Assert
            result.Should().BeEquivalentTo(singleDummy);
        }

        [Fact]
        public void SingleAsyncWithMoreThanOneThrowsException()
        {
            // Arrange
            var singleDummy = new Dummy
            {
                Id = Guid.NewGuid().ToString(),
                Name = "John"
            };

            var dummies = new List<Dummy>
            {
                new Dummy
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Nick"
                },
                singleDummy
            };
            Expression<Func<Dummy, bool>> predicate = x => true;

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Where(predicate);

            var entityStore = ResponseSetup(expected, dataSource);

            // Act
            var result = new Action(() => entityStore.Query().SingleAsync(predicate).GetAwaiter().GetResult());

            //Assert
            result.Should().Throw<InvalidOperationException>().WithMessage("Sequence contains more than one element");
        }

        [Fact]
        public void SingleAsyncWithNoItemsThrowsException()
        {
            // Arrange
            var dummies = new List<Dummy>();
            Expression<Func<Dummy, bool>> predicate = x => true;

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Where(predicate);

            var entityStore = ResponseSetup(expected, dataSource);

            // Act
            var result = new Action(() => entityStore.Query().SingleAsync(predicate).GetAwaiter().GetResult());

            //Assert
            result.Should().Throw<InvalidOperationException>().WithMessage("Sequence contains no elements");
        }

        [Fact]
        public void FirstAsyncWithNoItemsThrowsException()
        {
            // Arrange
            var dummies = new List<Dummy>();
            Expression<Func<Dummy, bool>> predicate = x => true;

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Where(predicate);

            var entityStore = ResponseSetup(expected, dataSource);

            // Act
            var result = new Action(() => entityStore.Query().FirstAsync(predicate).GetAwaiter().GetResult());

            //Assert
            result.Should().Throw<InvalidOperationException>().WithMessage("Sequence contains no elements");
        }

        [Fact]
        public async Task FirstOrDefaultAsyncWithNoItemsReturnsNull()
        {
            // Arrange
            var dummies = new List<Dummy>();
            Expression<Func<Dummy, bool>> predicate = x => true;

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Where(predicate);

            var entityStore = ResponseSetup(expected, dataSource);

            // Act
            var result = await entityStore.Query().FirstOrDefaultAsync(predicate);

            //Assert
            result.Should().BeNull();
        }

        private CosmosStore<Dummy> ResponseSetup(IQueryable<Dummy> expected, IQueryable<Dummy> dataSource)
        {
            FeedResponse<Dummy> response = new FeedResponse<Dummy>(expected);

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
            mockDocumentQuery.As<IQueryable<Dummy>>().Setup(x => x.GetEnumerator()).Returns(dataSource.GetEnumerator);


            _mockDocumentClient.Setup(x => x.CreateDocumentQuery<Dummy>(It.IsAny<string>(),
                    It.IsAny<FeedOptions>()))
                .Returns(mockDocumentQuery.Object);

            var entityStore = new CosmosStore<Dummy>(_mockDocumentClient.Object, "databaseName",
                new CosmosDatabaseCreator(_mockDocumentClient.Object),
                new CosmosCollectionCreator(_mockDocumentClient.Object));
            return entityStore;
        }

    }
}