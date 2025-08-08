// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using Dalog.Foundation.BackgroundServices.AzureEventHub;

namespace Dalog.Foundation.BackgroundServicesTests.AzureEventHubTests;

public class AzureEventHubBackgroundServiceOptionsTests
{
    [Fact]
    public void Options_ShouldHaveCorrectDefaultValues()
    {
        // Arrange & Act
        var options = new AzureEventHubBackgroundServiceOptions<TestMessage>();

        // Assert
        Assert.Equal(string.Empty, options.StorageConnectionString);
        Assert.Equal(string.Empty, options.StorageContainerName);
        Assert.Equal(string.Empty, options.EventHubConnectionString);
        Assert.Equal(300, options.PrefetchCount);
        Assert.Equal(TimeSpan.FromSeconds(10), options.MaximumWaitTime);
        Assert.Equal(TimeSpan.FromSeconds(30), options.LoadBalancingUpdateInterval);
        Assert.Equal("$Default", options.ConsumerGroupName);
    }

    [Fact]
    public void Options_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var options = new AzureEventHubBackgroundServiceOptions<TestMessage>();
        const string expectedStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;";
        const string expectedStorageContainerName = "test-container";
        const string expectedEventHubConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test;EntityPath=test";
        const int expectedPrefetchCount = 100;
        var expectedMaximumWaitTime = TimeSpan.FromSeconds(30);
        var expectedLoadBalancingUpdateInterval = TimeSpan.FromMinutes(1);
        const string expectedConsumerGroupName = "test-group";

        // Act
        options.StorageConnectionString = expectedStorageConnectionString;
        options.StorageContainerName = expectedStorageContainerName;
        options.EventHubConnectionString = expectedEventHubConnectionString;
        options.PrefetchCount = expectedPrefetchCount;
        options.MaximumWaitTime = expectedMaximumWaitTime;
        options.LoadBalancingUpdateInterval = expectedLoadBalancingUpdateInterval;
        options.ConsumerGroupName = expectedConsumerGroupName;

        // Assert
        Assert.Equal(expectedStorageConnectionString, options.StorageConnectionString);
        Assert.Equal(expectedStorageContainerName, options.StorageContainerName);
        Assert.Equal(expectedEventHubConnectionString, options.EventHubConnectionString);
        Assert.Equal(expectedPrefetchCount, options.PrefetchCount);
        Assert.Equal(expectedMaximumWaitTime, options.MaximumWaitTime);
        Assert.Equal(expectedLoadBalancingUpdateInterval, options.LoadBalancingUpdateInterval);
        Assert.Equal(expectedConsumerGroupName, options.ConsumerGroupName);
    }

    [Fact]
    public void Validate_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var options = new AzureEventHubBackgroundServiceOptions<TestMessage>
        {
            StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;",
            StorageContainerName = "test-container",
            EventHubConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test;EntityPath=test",
            PrefetchCount = 100
        };

        // Act & Assert - Should not throw
        options.Validate();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidStorageConnectionString_ShouldThrowInvalidOperationException(string? storageConnectionString)
    {
        // Arrange
        var options = new AzureEventHubBackgroundServiceOptions<TestMessage>
        {
            StorageConnectionString = storageConnectionString!,
            StorageContainerName = "test-container",
            EventHubConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test;EntityPath=test"
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Equal("Storage connection string must be provided.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidStorageContainerName_ShouldThrowInvalidOperationException(string? storageContainerName)
    {
        // Arrange
        var options = new AzureEventHubBackgroundServiceOptions<TestMessage>
        {
            StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;",
            StorageContainerName = storageContainerName!,
            EventHubConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test;EntityPath=test"
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Equal("Storage container name must be provided.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidEventHubConnectionString_ShouldThrowInvalidOperationException(string? eventHubConnectionString)
    {
        // Arrange
        var options = new AzureEventHubBackgroundServiceOptions<TestMessage>
        {
            StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;",
            StorageContainerName = "test-container",
            EventHubConnectionString = eventHubConnectionString!
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Equal("Event Hub connection string must be provided.", exception.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Validate_WithNegativePrefetchCount_ShouldThrowInvalidOperationException(int prefetchCount)
    {
        // Arrange
        var options = new AzureEventHubBackgroundServiceOptions<TestMessage>
        {
            StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;",
            StorageContainerName = "test-container",
            EventHubConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test;EntityPath=test",
            PrefetchCount = prefetchCount
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Equal("Prefetch count must be greater than zero.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Validate_WithValidPrefetchCount_ShouldNotThrow(int prefetchCount)
    {
        // Arrange
        var options = new AzureEventHubBackgroundServiceOptions<TestMessage>
        {
            StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;",
            StorageContainerName = "test-container",
            EventHubConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test;EntityPath=test",
            PrefetchCount = prefetchCount
        };

        // Act & Assert - Should not throw
        options.Validate();
    }

    [Fact]
    public void Validate_WithInvalidMaximumWaitTime_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = new AzureEventHubBackgroundServiceOptions<TestMessage>
        {
            StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;",
            StorageContainerName = "test-container",
            EventHubConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test;EntityPath=test",
            MaximumWaitTime = TimeSpan.Zero
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Equal("Maximum wait time must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Validate_WithInvalidLoadBalancingUpdateInterval_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = new AzureEventHubBackgroundServiceOptions<TestMessage>
        {
            StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;",
            StorageContainerName = "test-container",
            EventHubConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test;EntityPath=test",
            LoadBalancingUpdateInterval = TimeSpan.Zero
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Equal("Load balancing update interval must be greater than zero.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidConsumerGroupName_ShouldThrowInvalidOperationException(string? consumerGroupName)
    {
        // Arrange
        var options = new AzureEventHubBackgroundServiceOptions<TestMessage>
        {
            StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;",
            StorageContainerName = "test-container",
            EventHubConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test;EntityPath=test",
            ConsumerGroupName = consumerGroupName!
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Equal("Consumer group name must be provided.", exception.Message);
    }

    [Fact]
    public void Validate_WithMultipleInvalidProperties_ShouldThrowForFirstEncounteredError()
    {
        // Arrange
        var options = new AzureEventHubBackgroundServiceOptions<TestMessage>
        {
            StorageConnectionString = "", // Invalid
            StorageContainerName = "", // Also invalid
            EventHubConnectionString = "", // Also invalid
            PrefetchCount = -1 // Also invalid
        };

        // Act & Assert
        // Should throw for storage connection string first (as it's validated first)
        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Equal("Storage connection string must be provided.", exception.Message);
    }

    // Test helper class
    private sealed class TestMessage
    {
        // Empty class for testing purposes
    }
}
