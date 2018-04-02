using System;
using Cosmonaut.Exceptions;
using Xunit;

namespace Cosmonaut.Tests
{
    public class EntityValidatorTests
    {
        [Fact]
        public void ObjectWithPropertyNamedIdAssignsCosmosId()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var processor = new CosmosDocumentProcessor<Dummy>();
            var dummy = new Dummy
            {
                Id = id,
                Name = "Test"
            };

            // Act
            processor.ValidateEntityForCosmosDb(dummy);
            var idFromDocument = processor.GetDocumentId(dummy);

            // Assert
            Assert.Equal(id, idFromDocument);
        }

        [Fact]
        public void ObjectImplimentingCosmosEntityAssignsCosmosId()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var processor = new CosmosDocumentProcessor<DummyImplEntity>();
            var dummy = new DummyImplEntity
            {
                CosmosId = id,
                Name = "Test"
            };

            // Act
            processor.ValidateEntityForCosmosDb(dummy);
            var idFromDocument = processor.GetDocumentId(dummy);

            // Assert
            Assert.Equal(id, idFromDocument);
        }
        
        [Fact]
        public void ObjectImplimentingCosmosEntityWithExistingIdThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var processor = new CosmosDocumentProcessor<DummyImplEntityWithAttr>();
            var dummy = new DummyImplEntityWithAttr
            {
                CosmosId = id,
                Name = "Test",
                Id = id
            };

            // Act & Assert
            Assert.Throws<MultipleCosmosIdsException>(() => processor.ValidateEntityForCosmosDb(dummy));
        }

        [Fact]
        public void ObjectWithIdAndAttributeThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var processor = new CosmosDocumentProcessor<DummyWithIdAndWithAttr>();
            var dummy = new DummyWithIdAndWithAttr
            {
                ActualyId = id,
                Name = "Test",
                Id = id
            };

            // Act & Assert
            Assert.Throws<MultipleCosmosIdsException>(() => processor.ValidateEntityForCosmosDb(dummy));
        }

        [Fact]
        public void ObjectMultipleAttributesThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var processor = new CosmosDocumentProcessor<DummyWithMultipleAttr>();
            var dummy = new DummyWithMultipleAttr
            {
                ActualyId = id,
                Name = "Test",
                Id = id
            };

            // Act & Assert
            Assert.Throws<MultipleCosmosIdsException>(() => processor.ValidateEntityForCosmosDb(dummy));
        }

        [Fact]
        public void ObjectWithIdPropertyAndAttributeOnThatPropertyAssigns()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var processor = new CosmosDocumentProcessor<DummyWithIdAttrOnId>();
            var dummy = new DummyWithIdAttrOnId
            {
                Id = id,
                Name = "Test"
            };

            // Act
            processor.ValidateEntityForCosmosDb(dummy);
            var idFromDocument = processor.GetDocumentId(dummy);

            // Assert
            Assert.Equal(id, idFromDocument);
        }

        [Fact]
        public void ObjectWithoutAnyIdThrowsException()
        {
            // Arrange
            var processor = new CosmosDocumentProcessor<object>();
            var dummy = new
            {
                Name = "Test"
            };

            // Act & Assert
            Assert.Throws<CosmosEntityWithoutIdException<object>>(() =>
            {
                processor.ValidateEntityForCosmosDb(dummy);
                processor.GetDocumentId(dummy);
            });
        }
    }
}