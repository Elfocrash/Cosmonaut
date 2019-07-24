using System;
using System.Collections.Generic;
using Cosmonaut.Exceptions;
using Cosmonaut.Extensions;
using FluentAssertions;
using Xunit;

namespace Cosmonaut.Unit
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
            result.Should().Be(expectedQuery);
        }

        [Fact]
        public void NotSharedCollectionSqlQueryWithoutWhereClauseDoesNotAddCosmosEntityName()
        {
            // Arrange
            var expectedQuery = $"select * from c";

            // Act
            var result = "select * from c".EnsureQueryIsCollectionSharingFriendly<Dummy>();

            // Assert
            result.Should().Be(expectedQuery);
        }

        [Fact]
        public void SharedCollectionSqlQueryWithWhereClauseAddsCosmosEntityName()
        {
            // Arrange
            var expectedQuery = $"select * from c where c.{nameof(ISharedCosmosEntity.CosmosEntityName)} = '{typeof(DummySharedCollection).GetSharedCollectionEntityName()}' and c.id = '1'";

            // Act
            var result = "select * from c where c.id = '1'".EnsureQueryIsCollectionSharingFriendly<DummySharedCollection>();

            // Assert
            result.Should().Be(expectedQuery);
        }

        [Fact]
        public void NotSharedCollectionSqlQueryWithWhereClauseDoesNotAddCosmosEntityName()
        {
            // Arrange
            var expectedQuery = $"select * from c where c.id = '1'";

            // Act
            var result = "select * from c where c.id = '1'".EnsureQueryIsCollectionSharingFriendly<Dummy>();

            // Assert
            result.Should().Be(expectedQuery);
        }

        [Fact]
        public void SharedCollectionSqlQueryWithWhereClauseAndAsAddsCosmosEntityName()
        {
            // Arrange
            var expectedQuery = $"select * from root as c where c.{nameof(ISharedCosmosEntity.CosmosEntityName)} = '{typeof(DummySharedCollection).GetSharedCollectionEntityName()}' and c.id = '1'";

            // Act
            var result = "select * from root as c where c.id = '1'".EnsureQueryIsCollectionSharingFriendly<DummySharedCollection>();

            // Assert
            result.Should().Be(expectedQuery);
        }

        [Fact]
        public void SharedCollectionSqlQueryWithWhereClauseWithoutAsAddsCosmosEntityName()
        {
            // Arrange
            var expectedQuery = $"select * from root c where c.{nameof(ISharedCosmosEntity.CosmosEntityName)} = '{typeof(DummySharedCollection).GetSharedCollectionEntityName()}' and c.id = '1'";

            // Act
            var result = "select * from root c where c.id = '1'".EnsureQueryIsCollectionSharingFriendly<DummySharedCollection>();

            // Assert
            result.Should().Be(expectedQuery);
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

        [Fact]
        public void ConvertToSqlParameterCollection_WhenValidObject_ReturnsCorrectCollection()
        {
            // Arrange
            var obj = new {Cosmonaut = "Nick", Position = "Software Engineer", @Rank = 1};

            // Act
            var collection = obj.ConvertToSqlParameterCollection();

            // Assert
            collection.Count.Should().Be(3);
            collection[0].Name.Should().Be($"@{nameof(obj.Cosmonaut)}");
            collection[0].Value.Should().Be("Nick");
            collection[1].Name.Should().Be($"@{nameof(obj.Position)}");
            collection[1].Value.Should().Be("Software Engineer");
            collection[2].Name.Should().Be($"@{nameof(obj.Rank)}");
            collection[2].Value.Should().Be(1);
        }

        [Fact]
        public void ConvertDictionaryToSqlParameterCollection_WhenValidObject_ReturnsCorrectCollection()
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
            {
                {"Cosmonaut", "Nick"}, {"@Position", "Software Engineer"}, {"Rank", 1}
            };

            // Act
            var collection = dictionary.ConvertDictionaryToSqlParameterCollection();

            // Assert
            collection.Count.Should().Be(3);
            collection[0].Name.Should().Be("@Cosmonaut");
            collection[0].Value.Should().Be("Nick");
            collection[1].Name.Should().Be("@Position");
            collection[1].Value.Should().Be("Software Engineer");
            collection[2].Name.Should().Be("@Rank");
            collection[2].Value.Should().Be(1);
        }

        
        [Fact]
        public void SharedCollectionSqlQueryWithUpperCaseOrderClauseWithoutAsAddsCosmosEntityName()
        {
            // Arrange
            var expectedQuery = "select * from c where c.CosmosEntityName = 'dummies' order by c.id";

            // Act
            var result = "select * from c ORDER BY c.id".EnsureQueryIsCollectionSharingFriendly<DummySharedCollection>();

            // Assert
            result.Should().Be(expectedQuery);
        }
    }
}