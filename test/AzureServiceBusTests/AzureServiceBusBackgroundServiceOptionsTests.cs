// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using Dalog.Foundation.BackgroundServices.AzureServiceBus;

namespace Dalog.Foundation.BackgroundServicesTests.AzureServiceBusTests;

public class AzureServiceBusBackgroundServiceOptionsTests
{
    [Fact]
    public void Options_ShouldHaveCorrectDefaultValues()
    {
        // Arrange & Act
        var options = new AzureServiceBusBackgroundServiceOptions<TestMessage>();

        // Assert
        Assert.Equal(string.Empty, options.ConnectionString);
        Assert.Equal(string.Empty, options.QueueName);
        Assert.Equal(0, options.PrefetchCount);
    }

    [Fact]
    public void Options_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var options = new AzureServiceBusBackgroundServiceOptions<TestMessage>();
        const string expectedConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test";
        const string expectedQueueName = "test-queue";
        const int expectedPrefetchCount = 10;

        // Act
        options.ConnectionString = expectedConnectionString;
        options.QueueName = expectedQueueName;
        options.PrefetchCount = expectedPrefetchCount;

        // Assert
        Assert.Equal(expectedConnectionString, options.ConnectionString);
        Assert.Equal(expectedQueueName, options.QueueName);
        Assert.Equal(expectedPrefetchCount, options.PrefetchCount);
    }

    [Fact]
    public void Validate_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var options = new AzureServiceBusBackgroundServiceOptions<TestMessage>
        {
            ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
            QueueName = "test-queue",
            PrefetchCount = 5
        };

        // Act & Assert - Should not throw
        options.Validate();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidConnectionString_ShouldThrowInvalidOperationException(string? connectionString)
    {
        // Arrange
        var options = new AzureServiceBusBackgroundServiceOptions<TestMessage>
        {
            ConnectionString = connectionString!,
            QueueName = "test-queue",
            PrefetchCount = 0
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Equal("Azure Service Bus connection string is not configured.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidQueueName_ShouldThrowInvalidOperationException(string? queueName)
    {
        // Arrange
        var options = new AzureServiceBusBackgroundServiceOptions<TestMessage>
        {
            ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
            QueueName = queueName!,
            PrefetchCount = 0
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Equal("Azure Service Bus queue name is not configured.", exception.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Validate_WithNegativePrefetchCount_ShouldThrowInvalidOperationException(int prefetchCount)
    {
        // Arrange
        var options = new AzureServiceBusBackgroundServiceOptions<TestMessage>
        {
            ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
            QueueName = "test-queue",
            PrefetchCount = prefetchCount
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Equal("Azure Service Bus prefetch count must be zero or greater.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Validate_WithValidPrefetchCount_ShouldNotThrow(int prefetchCount)
    {
        // Arrange
        var options = new AzureServiceBusBackgroundServiceOptions<TestMessage>
        {
            ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
            QueueName = "test-queue",
            PrefetchCount = prefetchCount
        };

        // Act & Assert - Should not throw
        options.Validate();
    }

    [Fact]
    public void Validate_WithMultipleInvalidProperties_ShouldThrowForFirstEncounteredError()
    {
        // Arrange
        var options = new AzureServiceBusBackgroundServiceOptions<TestMessage>
        {
            ConnectionString = "", // Invalid
            QueueName = "", // Also invalid
            PrefetchCount = -1 // Also invalid
        };

        // Act & Assert
        // Should throw for connection string first (as it's validated first)
        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Equal("Azure Service Bus connection string is not configured.", exception.Message);
    }

    // Test helper class
    private sealed class TestMessage
    {
        // Empty class for testing purposes
    }
}
