using System;
using System.Threading.Tasks;
using Cosmonaut.Exceptions;
using Cosmonaut.Response;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosAddTests
    {
        private readonly ICosmosStore<Dummy> _dummyStore;

        public CosmosAddTests()
        {
            _dummyStore = new InMemoryCosmosStore<Dummy>();
        }

        [Fact]
        public async Task AddValidObjectSuccess()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummy = new Dummy
            {
                Id = id,
                Name = "Nick"
            };

            // Act
            var expectedResponse = new CosmosResponse<Dummy>(dummy, CosmosOperationStatus.Success);
            var result = await _dummyStore.AddAsync(dummy);
            
            //Assert
            Assert.Equal(expectedResponse.Entity, result.Entity);
            Assert.Equal(expectedResponse.IsSuccess, result.IsSuccess);
        }

        [Fact]
        public async Task AddEntityWithoutIdEmptyGeneratedId()
        {
            // Arrange
            var dummy = new Dummy
            {
                Name = "Nick"
            };

            // Act
            var result = await _dummyStore.AddAsync(dummy);

            //Assert
            var isGuid = Guid.TryParse(result.Entity.Id, out var guid);
            Assert.True(isGuid);
            Assert.NotEqual(Guid.Empty, guid);
        }

        [Fact]
        public async Task AddingEntityWithoutIdThrowsException()
        {
            // Arrange
            var dummy = new
            {
                Name = "Name"
            };

            // Act
            var addTask = new InMemoryCosmosStore<object>().AddAsync(dummy);

            //Assert
            await Assert.ThrowsAsync<CosmosEntityWithoutIdException<object>>(() => addTask);
        }
    }
}