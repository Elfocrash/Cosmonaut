using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Response;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosRemoveTests
    {
        private readonly ICosmosStore<Dummy> _dummyStore;

        public CosmosRemoveTests()
        {
            _dummyStore = new InMemoryCosmosStore<Dummy>();
        }

        [Fact]
        public async Task RemoveEntityRemoves()
        {
            // Assign
            var id = Guid.NewGuid().ToString();
            var addedDummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };
            await _dummyStore.AddAsync(addedDummy);

            // Act
            var result = await _dummyStore.RemoveAsync(addedDummy);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CosmosOperationStatus.Success, result.CosmosOperationStatus);
        }

        [Fact]
        public async Task RemoveByIdRemoves()
        {
            // Assign
            var id = Guid.NewGuid().ToString();
            var addedDummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };
            await _dummyStore.AddAsync(addedDummy);

            // Act
            var result = await _dummyStore.RemoveByIdAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(CosmosOperationStatus.Success, result.CosmosOperationStatus);
        }

        [Fact]
        public async Task RemoveByExpressionRemoves()
        {
            // Assign
            foreach (var i in Enumerable.Range(0, 10))
            {
                var id = Guid.NewGuid().ToString();
                var addedDummy = new Dummy
                {
                    Id = id,
                    Name = "Test " + i
                };
                await _dummyStore.AddAsync(addedDummy);
            }

            // Act
            var result = await _dummyStore.RemoveAsync(x => x.Name.Contains("Test"));

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.FailedEntities);
        }

        [Fact]
        public async Task RemoveRangeRemoves()
        {
            // Assign
            var addedList = new List<Dummy>();
            foreach (var i in Enumerable.Range(0, 10))
            {
                var id = Guid.NewGuid().ToString();
                var addedDummy = new Dummy
                {
                    Id = id,
                    Name = "Test " + i
                };
                await _dummyStore.AddAsync(addedDummy);
                addedList.Add(addedDummy);
            }

            // Act
            var result = await _dummyStore.RemoveRangeAsync(addedList);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.FailedEntities);
        }
    }
}