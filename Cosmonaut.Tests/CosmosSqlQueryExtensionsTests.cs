using System;
using Cosmonaut.Exceptions;
using Cosmonaut.Extensions;
using FluentAssertions;
using Xunit;

namespace Cosmonaut.Tests
{
    public class CosmosSqlQueryExtensionsTests
    {
        [Fact]
        public void SharedCollectionSqlQueryWithoutWhereClauseAddsCosmosEntityName()
        {
            // Arrange
            var expectedQuery = $"select * from c where c.{nameof(ISharedCosmosEntity.CosmosEntityName)} = '{typeof(DummySharedCollection).GetSharedCollectionEntityName()}'";
            
            // Act
            var result = "select * from c".EnsureQueryIsCollectionSharingFriendly<DummySharedCollection>();

            // Assert
            result.Should().BeEquivalentTo(expectedQuery);
        }

        [Fact]
        public void NotSharedCollectionSqlQueryWithoutWhereClauseDoesNotAddCosmosEntityName()
        {
            // Arrange
            var expectedQuery = $"select * from c";

            // Act
            var result = "select * from c".EnsureQueryIsCollectionSharingFriendly<Dummy>();

            // Assert
            result.Should().BeEquivalentTo(expectedQuery);
        }

        [Fact]
        public void SharedCollectionSqlQueryWithWhereClauseAddsCosmosEntityName()
        {
            // Arrange
            var expectedQuery = $"select * from c where c.{nameof(ISharedCosmosEntity.CosmosEntityName)} = '{typeof(DummySharedCollection).GetSharedCollectionEntityName()}' and c.id = '1'";

            // Act
            var result = "select * from c where c.id = '1'".EnsureQueryIsCollectionSharingFriendly<DummySharedCollection>();

            // Assert
            result.Should().BeEquivalentTo(expectedQuery);
        }

        [Fact]
        public void NotSharedCollectionSqlQueryWithWhereClauseDoesNotAddCosmosEntityName()
        {
            // Arrange
            var expectedQuery = $"select * from c where c.id = '1'";

            // Act
            var result = "select * from c where c.id = '1'".EnsureQueryIsCollectionSharingFriendly<Dummy>();

            // Assert
            result.Should().BeEquivalentTo(expectedQuery);
        }

        [Fact]
        public void SharedCollectionSqlQueryWithWhereClauseAndAsAddsCosmosEntityName()
        {
            // Arrange
            var expectedQuery = $"select * from root as c where c.{nameof(ISharedCosmosEntity.CosmosEntityName)} = '{typeof(DummySharedCollection).GetSharedCollectionEntityName()}' and c.id = '1'";

            // Act
            var result = "select * from root as c where c.id = '1'".EnsureQueryIsCollectionSharingFriendly<DummySharedCollection>();

            // Assert
            result.Should().BeEquivalentTo(expectedQuery);
        }

        [Fact]
        public void SharedCollectionSqlQueryWithWhereClauseWithoutAsAddsCosmosEntityName()
        {
            // Arrange
            var expectedQuery = $"select * from root c where c.{nameof(ISharedCosmosEntity.CosmosEntityName)} = '{typeof(DummySharedCollection).GetSharedCollectionEntityName()}' and c.id = '1'";

            // Act
            var result = "select * from root c where c.id = '1'".EnsureQueryIsCollectionSharingFriendly<DummySharedCollection>();

            // Assert
            result.Should().BeEquivalentTo(expectedQuery);
        }

        [Fact]
        public void SqlQueryWithKeywordAsCollectionNameThrowsException()
        {
            // Arrange
            var query = "select * from as";

            // Act
            var action = new Action(() => query.EnsureQueryIsCollectionSharingFriendly<DummySharedCollection>());

            // Assert
            action.Should().Throw<InvalidSqlQueryException>();
        }

        [Fact]
        public void SqlQueryWithKeywordAsCollectionNameAndWhereClauseThrowsException()
        {
            // Arrange
            var query = "select * from root as where as.id = '1'";

            // Act
            var action = new Action(() => query.EnsureQueryIsCollectionSharingFriendly<DummySharedCollection>());

            // Assert
            action.Should().Throw<InvalidSqlQueryException>();
        }
    }
}