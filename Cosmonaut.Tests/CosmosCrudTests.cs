using System;
using System.Threading.Tasks;
using Cosmonaut.Response;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosCrudTests
    {
        private readonly ICosmosStore<Dummy> _dummyStore;

        public CosmosCrudTests()
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
    }
}