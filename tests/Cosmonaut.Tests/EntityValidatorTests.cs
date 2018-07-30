using System;
using Cosmonaut.Exceptions;
using Cosmonaut.Extensions;
using Xunit;

namespace Cosmonaut.Unit
{
    public class EntityValidatorTests
    {
        [Fact]
        public void ObjectWithPropertyNamedIdAssignsCosmosId()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };

            // Act
            dummy.ValidateEntityForCosmosDb();
            var idFromDocument = dummy.GetDocumentId();

            // Assert
            Assert.Equal(id, idFromDocument);
        }

        [Fact]
        public void ObjectImplimentingCosmosEntityAssignsCosmosId()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummy = new DummyImplEntity
            {
                CosmosId = id,
                Name = "Test"
            };

            // Act
            dummy.ValidateEntityForCosmosDb();
            var idFromDocument = dummy.GetDocumentId();

            // Assert
            Assert.Equal(id, idFromDocument);
        }
        
        [Fact]
        public void ObjectImplimentingCosmosEntityWithExistingIdThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummy = new DummyImplEntityWithAttr
            {
                CosmosId = id,
                Name = "Test",
                Id = id
            };

            // Act & Assert
            Assert.Throws<MultipleCosmosIdsException>(() => dummy.ValidateEntityForCosmosDb());
        }

        [Fact]
        public void ObjectWithIdAndAttributeThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummy = new DummyWithIdAndWithAttr
            {
                ActualyId = id,
                Name = "Test",
                Id = id
            };

            // Act & Assert
            Assert.Throws<MultipleCosmosIdsException>(() => dummy.ValidateEntityForCosmosDb());
        }

        [Fact]
        public void ObjectMultipleAttributesThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummy = new DummyWithMultipleAttr
            {
                ActualyId = id,
                Name = "Test",
                Id = id
            };

            // Act & Assert
            Assert.Throws<MultipleCosmosIdsException>(() => dummy.ValidateEntityForCosmosDb());
        }

        [Fact]
        public void ObjectWithIdPropertyAndAttributeOnThatPropertyAssigns()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var dummy = new DummyWithIdAttrOnId
            {
                Id = id,
                Name = "Test"
            };

            // Act
            dummy.ValidateEntityForCosmosDb();
            var idFromDocument = dummy.GetDocumentId();

            // Assert
            Assert.Equal(id, idFromDocument);
        }

        [Fact]
        public void ObjectWithoutAnyIdThrowsException()
        {
            // Arrange
            var dummy = new DummyNoId
            {
                Name = "Test"
            };

            // Act & Assert
            Assert.Throws<CosmosEntityWithoutIdException<DummyNoId>>(() =>
            {
                dummy.ValidateEntityForCosmosDb();
                dummy.GetDocumentId();
            });
        }
    }
}