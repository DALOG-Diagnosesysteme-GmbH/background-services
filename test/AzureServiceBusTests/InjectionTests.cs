// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using Dalog.Foundation.BackgroundServices.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Dalog.Foundation.BackgroundServicesTests.AzureServiceBusTests;

public class InjectionTests
{
    [Fact]
    public void AddAzureServiceBusBackgroundService_ShouldRegisterAllServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(); // Add logging for background service

        // Act
        serviceCollection.AddAzureServiceBusBackgroundService<TestMessage, TestAzureServiceBusHandler>(options =>
        {
            options.ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test";
            options.QueueName = "test-queue";
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetRequiredService<TestAzureServiceBusHandler>());
        Assert.NotNull(serviceProvider.GetRequiredService<IOptions<AzureServiceBusBackgroundServiceOptions<TestMessage>>>());
        Assert.NotNull(serviceProvider.GetServices<IHostedService>().FirstOrDefault(s => s.GetType().Name.Contains("AzureServiceBusBackgroundService")));
    }

    [Fact]
    public void AddAzureServiceBusBackgroundService_WithConfiguration_ShouldApplyOptions()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        const string expectedConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test";
        const string expectedQueueName = "test-queue";
        const int expectedPrefetchCount = 10;

        // Act
        serviceCollection.AddAzureServiceBusBackgroundService<TestMessage, TestAzureServiceBusHandler>(options =>
        {
            options.ConnectionString = expectedConnectionString;
            options.QueueName = expectedQueueName;
            options.PrefetchCount = expectedPrefetchCount;
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<AzureServiceBusBackgroundServiceOptions<TestMessage>>>();
        Assert.Equal(expectedConnectionString, options.Value.ConnectionString);
        Assert.Equal(expectedQueueName, options.Value.QueueName);
        Assert.Equal(expectedPrefetchCount, options.Value.PrefetchCount);
    }

    [Fact]
    public void AddAzureServiceBusBackgroundService_WithConnectionStringAndQueueOverload_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        const string expectedConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test";
        const string expectedQueueName = "test-queue";

        // Act
        serviceCollection.AddAzureServiceBusBackgroundService<TestMessage, TestAzureServiceBusHandler>(
            expectedConnectionString, expectedQueueName);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<AzureServiceBusBackgroundServiceOptions<TestMessage>>>();
        Assert.Equal(expectedConnectionString, options.Value.ConnectionString);
        Assert.Equal(expectedQueueName, options.Value.QueueName);
        Assert.Equal(0, options.Value.PrefetchCount); // Default value
    }

    [Fact]
    public void AddAzureServiceBusBackgroundService_WithoutConfiguration_ShouldUseDefaultOptions()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddAzureServiceBusBackgroundService<TestMessage, TestAzureServiceBusHandler>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<AzureServiceBusBackgroundServiceOptions<TestMessage>>>();
        Assert.Equal(string.Empty, options.Value.ConnectionString); // Default value
        Assert.Equal(string.Empty, options.Value.QueueName); // Default value
        Assert.Equal(0, options.Value.PrefetchCount); // Default value
    }

    [Fact]
    public void AddAzureServiceBusBackgroundService_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        ServiceCollection? serviceCollection = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            serviceCollection!.AddAzureServiceBusBackgroundService<TestMessage, TestAzureServiceBusHandler>());
    }

    // Test helper classes
    private sealed class TestMessage
    {
        // Empty class for testing purposes
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class TestAzureServiceBusHandler : IAzureServiceBusHandler<TestMessage>
    {
        public Task Handle(TestMessage message, IReadOnlyDictionary<string, object> applicationProperties,
            CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
