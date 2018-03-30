using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cosmonaut.Response;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosUpdateTests
    {
        private readonly ICosmosStore<Dummy> _dummyStore;

        public CosmosUpdateTests()
        {
            _dummyStore = new InMemoryCosmosStore<Dummy>();
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
            await _dummyStore.AddAsync(addedDummy);

            // Act
            addedDummy.Name = expectedName;
            var result = await _dummyStore.UpdateAsync(addedDummy);

            // Assert
            Assert.Equal(expectedName, result.Entity.Name);
        }

        [Fact]
        public async Task UpdateRangeUpdatesEntities()
        {
            // Arrange
            var addedEntities = new List<Dummy>();
            for (int i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid().ToString();
                var addedDummy = new Dummy
                {
                    Id = id,
                    Name = "UpdateMe"
                };
                var added = await _dummyStore.AddAsync(addedDummy);
                addedEntities.Add(added.Entity);
            }
            
            // Act
            var result = await _dummyStore.UpdateRangeAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.FailedEntities);
        }
        
        [Fact]
        public async Task UpdateEntityThatHasIdChangedFails()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var addedDummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };
            await _dummyStore.AddAsync(addedDummy);

            // Act
            addedDummy.Id = Guid.NewGuid().ToString();
            var result = await _dummyStore.UpdateAsync(addedDummy);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CosmosOperationStatus.ResourceNotFound, result.CosmosOperationStatus);
        }
    }
}