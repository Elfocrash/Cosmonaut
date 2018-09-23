using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Moq;
using Xunit;

namespace Cosmonaut.Unit
{
    public class CosmosQueryTests
    {
        private Mock<IDocumentClient> _mockDocumentClient;

        public CosmosQueryTests()
        {
            _mockDocumentClient = MockHelpers.GetMockDocumentClient();
        }

        [Fact]
        public async Task ToListAsync_ReturnsList()
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

            MockHelpers.ResponseSetup(expected, dataSource, ref _mockDocumentClient);
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

            // Act
            var result = await entityStore.Query().Where(predicate).ToListAsync();

            //Assert
            result.Count.Should().Be(2);
            result.Should().BeEquivalentTo(dummies);
        }

        [Fact]
        public async Task QueryMultipleAsyncReturnsCorrectItems()
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

            var dataSource = dummies.AsQueryable();
            var entityStore = MockHelpers.ResponseSetupForQuery("select * from c", null, dataSource, dataSource, ref _mockDocumentClient);

            // Act
            var result = (await entityStore.QueryMultipleAsync("select * from c")).ToList();

            //Assert
            result.Count.Should().Be(2);
            result.Should().BeEquivalentTo(dummies);
        }

        [Fact]
        public async Task QuerySingleAsync_ReturnsSingleIdCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummies = new List<Dummy>
            {
                new Dummy
                {
                    Id = id,
                    Name = "Nick"
                },
                new Dummy
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "John"
                }
            };

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Where(x => x.Name == "Nick").Select(x => x.Id);
            var sql = "select value c.id from c where c.Name = 'Nick'";
            var entityStore = MockHelpers.ResponseSetupForQuery(sql, null, expected, dataSource, ref _mockDocumentClient);

            // Act
            var result = await entityStore.QuerySingleAsync<string>("select value c.id from c where c.Name = 'Nick'");

            //Assert
            result.Should().Be(id);
        }

        [Fact]
        public async Task QuerySingleAsync_ReturnsCorrectItem()
        {
            // Arrange
            var nickDummy = new Dummy
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Nick"
            };

            var dummies = new List<Dummy>
            {
                nickDummy,
                new Dummy
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "John"
                }
            };

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Where(x => x.Name == "Nick");

            var entityStore = MockHelpers.ResponseSetupForQuery("select top 1 * from c where c.Name = 'Nick'", null, expected, dataSource, ref _mockDocumentClient);

            // Act
            var result = await entityStore.QuerySingleAsync("select top 1 * from c where c.Name = 'Nick'");

            //Assert
            result.Should().BeEquivalentTo(nickDummy);
        }

        [Fact]
        public async Task QuerySingleAsync_WithObjectParameters_ReturnsSingleIdCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummies = new List<Dummy>
            {
                new Dummy
                {
                    Id = id,
                    Name = "Nick"
                },
                new Dummy
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "John"
                }
            };

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Where(x => x.Name == "Nick").Select(x => x.Id);
            var sql = "select value c.id from c where c.Name = @name";
            var obj = new {name = "Nick"};
            var entityStore = MockHelpers.ResponseSetupForQuery("select value c.id from c where c.Name = @name", null, expected, dataSource, ref _mockDocumentClient);

            // Act
            var result = await entityStore.QuerySingleAsync<string>(sql, obj);

            //Assert
            result.Should().Be(id);
        }

        [Fact]
        public async Task QueryMultipleAsyncReturnsIdAsStringCorrectly()
        {
            // Arrange
            var ids = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

            var dummies = new List<Dummy>
            {
                new Dummy
                {
                    Id = ids[0],
                    Name = "Nick"
                },
                new Dummy
                {
                    Id = ids[1],
                    Name = "John"
                }
            };

            var dataSource = dummies.AsQueryable();
            var expected = dataSource.Select(x => x.Id);

            var entityStore = MockHelpers.ResponseSetupForQuery("select value c.id from c", null, expected, dataSource, ref _mockDocumentClient);

            // Act
            var result = await entityStore.QueryMultipleAsync<string>("select value c.id from c");

            //Assert
            result.Should().BeEquivalentTo(ids);
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

            MockHelpers.ResponseSetup(expected, dataSource, ref _mockDocumentClient);
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

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

            MockHelpers.ResponseSetup(expected, dataSource, ref _mockDocumentClient);
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

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

            MockHelpers.ResponseSetup(expected, dataSource, ref _mockDocumentClient);
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

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

            MockHelpers.ResponseSetup(expected, dataSource, ref _mockDocumentClient);
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

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

            MockHelpers.ResponseSetup(expected, dataSource, ref _mockDocumentClient);
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

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

            MockHelpers.ResponseSetup(expected, dataSource, ref _mockDocumentClient);
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

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

            MockHelpers.ResponseSetup(expected, dataSource, ref _mockDocumentClient);
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

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

            MockHelpers.ResponseSetup(expected, dataSource, ref _mockDocumentClient);
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(_mockDocumentClient.Object), "databaseName");

            // Act
            var result = await entityStore.Query().FirstOrDefaultAsync(predicate);

            //Assert
            result.Should().BeNull();
        }
    }
}