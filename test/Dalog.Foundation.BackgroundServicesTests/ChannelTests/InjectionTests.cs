// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using Dalog.Foundation.BackgroundServices.Channel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Dalog.Foundation.BackgroundServicesTests.ChannelTests;

public class InjectionTests
{
    [Fact]
    public void AddChannelBackgroundService_ShouldRegisterAllServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(); // Add logging for background service

        // Act
        serviceCollection.AddChannelBackgroundService<string, TestChannelHandler>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetRequiredService<IChannelHandler<string>>());
        Assert.NotNull(serviceProvider.GetRequiredService<IChannelWriter<string>>());
        Assert.NotNull(serviceProvider.GetRequiredService<IChannelReader<string>>());
        Assert.NotNull(serviceProvider.GetRequiredService<IOptions<ChannelBackgroundServiceOptions<string>>>());
        Assert.NotNull(serviceProvider.GetServices<IHostedService>().FirstOrDefault(s => s.GetType().Name.Contains("ChannelBackgroundService")));
    }

    [Fact]
    public void AddChannelBackgroundService_WithConfiguration_ShouldApplyOptions()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        const int expectedTimeout = 45;
        const int expectedRetryAttempts = 5;

        // Act
        serviceCollection.AddChannelBackgroundService<string, TestChannelHandler>(options =>
        {
            options.TimeoutInMinutes = expectedTimeout;
            options.RetryAttempts = expectedRetryAttempts;
            options.DrainQueueOnShutdown = false;
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<ChannelBackgroundServiceOptions<string>>>();
        Assert.Equal(expectedTimeout, options.Value.TimeoutInMinutes);
        Assert.Equal(expectedRetryAttempts, options.Value.RetryAttempts);
        Assert.False(options.Value.DrainQueueOnShutdown);
    }

    [Fact]
    public void AddChannelBackgroundService_WithTimeoutOverload_ShouldSetTimeoutCorrectly()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        const int expectedTimeout = 60;

        // Act
        serviceCollection.AddChannelBackgroundService<string, TestChannelHandler>(expectedTimeout);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<ChannelBackgroundServiceOptions<string>>>();
        Assert.Equal(expectedTimeout, options.Value.TimeoutInMinutes);
    }

    [Fact]
    public void AddChannelBackgroundService_WithoutConfiguration_ShouldUseDefaultOptions()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddChannelBackgroundService<string, TestChannelHandler>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<ChannelBackgroundServiceOptions<string>>>();
        Assert.Equal(32, options.Value.TimeoutInMinutes); // Default value
        Assert.Equal(3, options.Value.RetryAttempts); // Default value
        Assert.True(options.Value.DrainQueueOnShutdown); // Default value
    }

    [Fact]
    public void AddChannelBackgroundService_ShouldRegisterSameChannelInstanceForReaderAndWriter()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddChannelBackgroundService<string, TestChannelHandler>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var reader = serviceProvider.GetRequiredService<IChannelReader<string>>();
        var writer = serviceProvider.GetRequiredService<IChannelWriter<string>>();
        
        // Both should be the same instance of ChannelService
        Assert.Same(reader, writer);
    }

    [Fact]
    public void AddChannelBackgroundService_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        ServiceCollection? serviceCollection = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            serviceCollection!.AddChannelBackgroundService<string, TestChannelHandler>());
    }

    // Test helper class
    private sealed class TestChannelHandler : IChannelHandler<string>
    {
        public Task Handle(string item, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
