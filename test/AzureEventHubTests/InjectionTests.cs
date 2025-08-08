// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using Dalog.Foundation.BackgroundServices.AzureEventHub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Dalog.Foundation.BackgroundServicesTests.AzureEventHubTests;

public class InjectionTests
{
    [Fact]
    public void AddAzureEventHubBackgroundService_ShouldRegisterAllServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(); // Add logging for background service

        // Act
        serviceCollection.AddAzureEventHubBackgroundService<TestMessage, TestAzureEventHubHandler>(options =>
        {
            options.StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;";
            options.StorageContainerName = "test-container";
            options.EventHubConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test;EntityPath=test";
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetRequiredService<TestAzureEventHubHandler>());
        Assert.NotNull(serviceProvider.GetRequiredService<IOptions<AzureEventHubBackgroundServiceOptions<TestMessage>>>());
        Assert.NotNull(serviceProvider.GetServices<IHostedService>().FirstOrDefault(s => s.GetType().Name.Contains("AzureEventHubBackgroundService")));
    }

    [Fact]
    public void AddAzureEventHubBackgroundService_WithConfiguration_ShouldApplyOptions()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        const string expectedStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;";
        const string expectedStorageContainerName = "test-container";
        const string expectedEventHubConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test;EntityPath=test";
        const int expectedPrefetchCount = 100;

        // Act
        serviceCollection.AddAzureEventHubBackgroundService<TestMessage, TestAzureEventHubHandler>(options =>
        {
            options.StorageConnectionString = expectedStorageConnectionString;
            options.StorageContainerName = expectedStorageContainerName;
            options.EventHubConnectionString = expectedEventHubConnectionString;
            options.PrefetchCount = expectedPrefetchCount;
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<AzureEventHubBackgroundServiceOptions<TestMessage>>>();
        Assert.Equal(expectedStorageConnectionString, options.Value.StorageConnectionString);
        Assert.Equal(expectedStorageContainerName, options.Value.StorageContainerName);
        Assert.Equal(expectedEventHubConnectionString, options.Value.EventHubConnectionString);
        Assert.Equal(expectedPrefetchCount, options.Value.PrefetchCount);
    }

    [Fact]
    public void AddAzureEventHubBackgroundService_WithoutConfiguration_ShouldUseDefaultOptions()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddAzureEventHubBackgroundService<TestMessage, TestAzureEventHubHandler>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<AzureEventHubBackgroundServiceOptions<TestMessage>>>();
        Assert.Equal(string.Empty, options.Value.StorageConnectionString); // Default value
        Assert.Equal(string.Empty, options.Value.StorageContainerName); // Default value
        Assert.Equal(string.Empty, options.Value.EventHubConnectionString); // Default value
        Assert.Equal(300, options.Value.PrefetchCount); // Default value
    }

    [Fact]
    public void AddAzureEventHubBackgroundService_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        ServiceCollection? serviceCollection = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            serviceCollection!.AddAzureEventHubBackgroundService<TestMessage, TestAzureEventHubHandler>());
    }

    // Test helper classes
    private sealed class TestMessage
    {
        public string Content { get; set; } = string.Empty;
    }

    private sealed class TestAzureEventHubHandler : IAzureEventHubHandler<TestMessage>
    {
        public Task Handle(TestMessage message, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
