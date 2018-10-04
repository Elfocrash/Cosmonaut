using System;
using Cosmonaut.Extensions;
using FluentAssertions;
using Xunit;

namespace Cosmonaut.Unit
{
    public class PaginationExtensionsTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void WithPagination_ThrowsError_WhenPageSizeIsNegativeOrZero(int pageSize)
        {
            // Arrange
            var mockDocumentClient = MockHelpers.GetMockDocumentClient();
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(() => mockDocumentClient.Object), "databaseName");

            // Act
            Action action = () => entityStore.Query().WithPagination(1, pageSize);

            // Assert
            action.Should().Throw<ArgumentOutOfRangeException>().WithMessage($"Page size must be a positive number.\r\nParameter name: {nameof(pageSize)}");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void WithPagination_ThrowsError_WhenPageNumberIsNegativeOrZero(int pageNumber)
        {
            // Arrange
            var mockDocumentClient = MockHelpers.GetMockDocumentClient();
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(() => mockDocumentClient.Object), "databaseName");

            // Act
            Action action = () => entityStore.Query().WithPagination(pageNumber, 1);

            // Assert
            action.Should().Throw<ArgumentOutOfRangeException>().WithMessage($"Page number must be a positive number.\r\nParameter name: pageNumber");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void WithPagination_ThrowsError_WhenPageSizeIsNegativeOrZeroWithToken(int pageSize)
        {
            // Arrange
            var mockDocumentClient = MockHelpers.GetMockDocumentClient();
            var entityStore = new CosmosStore<Dummy>(new CosmonautClient(() => mockDocumentClient.Object), "databaseName");

            // Act
            Action action = () => entityStore.Query().WithPagination(string.Empty, pageSize);

            // Assert
            action.Should().Throw<ArgumentOutOfRangeException>().WithMessage($"Page size must be a positive number.\r\nParameter name: {nameof(pageSize)}");
        }
    }
}