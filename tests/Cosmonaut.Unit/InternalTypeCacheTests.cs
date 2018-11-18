using System;
using System.Linq;
using System.Reflection;
using Cosmonaut.Internal;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Cosmonaut.Unit
{
    public class InternalTypeCacheTests
    {
        [Fact]
        public void GetPropertiesFromCache_SimpleObject_ReturnsProperties()
        {
            // Arrange
            var dummyProperties = typeof(Dummy).GetTypeInfo().GetProperties().ToList();

            // Act
            var props = InternalTypeCache.Instance.GetPropertiesFromCache(typeof(Dummy)).ToList();

            // Assert
            props.Should().BeEquivalentTo(dummyProperties);
        }

        [Fact]
        public void GetPropertiesFromCache_MultipleObjects_ReturnsProperties()
        {
            // Arrange
            var dummyProperties = typeof(Dummy).GetTypeInfo().GetProperties().ToList();

            // Act
            InternalTypeCache.Instance.GetPropertiesFromCache(typeof(Dummy));
            var props = InternalTypeCache.Instance.GetPropertiesFromCache(typeof(Dummy)).ToList();

            // Assert
            props.Should().BeEquivalentTo(dummyProperties);
        }

        [Fact]
        public void GetPropertiesFromCache_AnonymousType_ReturnsProperties()
        {
            // Arrange
            var anonymous = new {id = "1", Name = "Nick"};
            var dummyProperties = anonymous.GetType().GetTypeInfo().GetProperties().ToList();

            // Act
            var props = InternalTypeCache.Instance.GetPropertiesFromCache(anonymous.GetType()).ToList();

            // Assert
            props.Should().BeEquivalentTo(dummyProperties);
        }
    }
}