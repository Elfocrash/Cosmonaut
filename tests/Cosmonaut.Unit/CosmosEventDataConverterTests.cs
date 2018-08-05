using System;
using System.Collections.Generic;
using System.Linq;
using Cosmonaut.Diagnostics;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Cosmonaut.Unit
{
    public class CosmosEventDataConverterTests
    {
        [Fact]
        public void ConvertToDependencyFromEventData_Dictionary_ToCosmosEventMetadata()
        {
            // Arrange
            IDictionary<string, object> eventData = new Dictionary<string, object>();
            eventData["durationMilliseconds"] = 1000.0d;
            eventData["resultCode"] = "OK";
            eventData["startTime"] = DateTimeOffset.UtcNow;
            eventData["dependencyTypeName"] = nameof(CosmosEventDataConverterTests);
            eventData["dependencyName"] = "dependency";
            eventData["target"] = "theTarget";
            eventData["data"] = "This is some data. Cool ha?";
            eventData["isSuccess"] = true;
            eventData["properties"] = JsonConvert.SerializeObject(new { Property = "Value"});

            // Act
            var eventDataCosmos = CosmosEventDataConverter.ConvertToDependencyFromEventData(eventData);

            // Assert
            eventDataCosmos.Duration.Should().Be(TimeSpan.FromMilliseconds(1000));
            eventDataCosmos.StartTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
            eventDataCosmos.ResultCode.Should().Be("OK");
            eventDataCosmos.DependencyTypeName.Should().Be(nameof(CosmosEventDataConverterTests));
            eventDataCosmos.DependencyName.Should().Be("dependency");
            eventDataCosmos.Target.Should().Be("theTarget");
            eventDataCosmos.Data.Should().Be("This is some data. Cool ha?");
            eventDataCosmos.Success.Should().BeTrue();
            eventDataCosmos.Error.Should().BeNull();
            eventDataCosmos.Properties.Count.Should().Be(1);
            eventDataCosmos.Properties.Single().Key.Should().Be("Property");
            eventDataCosmos.Properties.Single().Value.Should().Be("Value");
        }

        [Fact]
        public void ConvertToDependencyFromEventData_Defaults_ToCosmosEventMetadata()
        {
            // Arrange
            IDictionary<string, object> eventData = new Dictionary<string, object>();
           
            // Act
            var eventDataCosmos = CosmosEventDataConverter.ConvertToDependencyFromEventData(eventData);

            // Assert
            eventDataCosmos.Duration.Should().Be(TimeSpan.Zero);
            eventDataCosmos.StartTime.Should().BeCloseTo(DateTimeOffset.UtcNow);
            eventDataCosmos.ResultCode.Should().BeEmpty();
            eventDataCosmos.DependencyTypeName.Should().Be("unknown");
            eventDataCosmos.DependencyName.Should().Be("unknown");
            eventDataCosmos.Target.Should().Be("unknown");
            eventDataCosmos.Data.Should().BeEmpty();
            eventDataCosmos.Success.Should().BeTrue();
            eventDataCosmos.Error.Should().BeNull();
            eventDataCosmos.Properties.Count.Should().Be(0);
        }
    }
}