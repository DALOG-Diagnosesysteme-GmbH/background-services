// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using Dalog.Foundation.BackgroundServices.Cron;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NCrontab;

namespace Dalog.Foundation.BackgroundServicesTests.CronTests;

public class InjectionTests
{
    [Fact]
    public void AddCronBackgroundService_ShouldRegisterAllServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(); // Add logging for hosted service

        // Act
        serviceCollection.AddCronBackgroundService<CronHandler>(options =>
        {
            options.CronExpression = "0 0 * * *"; // Valid cron expression
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetRequiredService<CronHandler>());
        Assert.NotNull(serviceProvider.GetRequiredService<IOptions<CronBackgroundServiceOptions<CronHandler>>>());
        Assert.NotNull(serviceProvider.GetServices<IHostedService>().FirstOrDefault(s => s.GetType().Name.Contains("CronBackgroundService")));
    }

    [Fact]
    public void AddCronBackgroundService_WithConfiguration_ShouldApplyOptions()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        const string expectedCronExpression = "0 0 12 * * *"; // Every day at noon (with seconds)
        const int expectedTimeout = 45;
        const bool expectedWaitForCompletion = false;
        const bool expectedIncludingSeconds = true;

        // Act
        serviceCollection.AddCronBackgroundService<CronHandler>(options =>
        {
            options.CronExpression = expectedCronExpression;
            options.TimeoutInMinutes = expectedTimeout;
            options.WaitForRequestCompletion = expectedWaitForCompletion;
            options.IncludingSeconds = expectedIncludingSeconds;
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<CronBackgroundServiceOptions<CronHandler>>>();
        Assert.Equal(expectedCronExpression, options.Value.CronExpression);
        Assert.Equal(expectedTimeout, options.Value.TimeoutInMinutes);
        Assert.Equal(expectedWaitForCompletion, options.Value.WaitForRequestCompletion);
        Assert.Equal(expectedIncludingSeconds, options.Value.IncludingSeconds);
    }

    [Fact]
    public void AddCronBackgroundService_WithCronExpressionOverload_ShouldSetCronExpressionCorrectly()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        const string expectedCronExpression = "0 12 * * *";

        // Act
        serviceCollection.AddCronBackgroundService<CronHandler>(expectedCronExpression);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<CronBackgroundServiceOptions<CronHandler>>>();
        Assert.Equal(expectedCronExpression, options.Value.CronExpression);
        Assert.False(options.Value.IncludingSeconds); // Default value
    }

    [Fact]
    public void AddCronBackgroundService_WithCronExpressionAndIncludingSeconds_ShouldSetBothCorrectly()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        const string expectedCronExpression = "0 0 12 * * *";
        const bool expectedIncludingSeconds = true;

        // Act
        serviceCollection.AddCronBackgroundService<CronHandler>(expectedCronExpression, expectedIncludingSeconds);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<CronBackgroundServiceOptions<CronHandler>>>();
        Assert.Equal(expectedCronExpression, options.Value.CronExpression);
        Assert.Equal(expectedIncludingSeconds, options.Value.IncludingSeconds);
    }

    [Fact]
    public void AddCronBackgroundService_WithoutConfiguration_ShouldUseDefaultOptions()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddCronBackgroundService<CronHandler>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<CronBackgroundServiceOptions<CronHandler>>>();
        Assert.Equal(string.Empty, options.Value.CronExpression); // Default value
        Assert.Equal(33, options.Value.TimeoutInMinutes); // Default value
        Assert.True(options.Value.WaitForRequestCompletion); // Default value
        Assert.False(options.Value.IncludingSeconds); // Default value
    }

    [Theory]
    [InlineData("a * * * *")]
    [InlineData("* * * *")]
    [InlineData("invalid")]
    [InlineData("60 * * * *")]
    [InlineData("* 24 * * *")]
    public void AddCronBackgroundService_WithInvalidCronExpression_ShouldThrowCrontabException(string invalidCronExpression)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act & Assert
        Assert.Throws<CrontabException>(() =>
            serviceCollection.AddCronBackgroundService<CronHandler>(invalidCronExpression));
    }

    [Theory]
    [InlineData("0 * * * *")]
    [InlineData("0 0 * * *")]
    [InlineData("0 0 1 * *")]
    [InlineData("*/5 * * * *")]
    [InlineData("0,15,30,45 * * * *")]
    public void AddCronBackgroundService_WithValidCronExpression_ShouldNotThrow(string validCronExpression)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act & Assert - Should not throw
        serviceCollection.AddCronBackgroundService<CronHandler>(validCronExpression);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<CronBackgroundServiceOptions<CronHandler>>>();
        Assert.Equal(validCronExpression, options.Value.CronExpression);
    }

    [Fact]
    public void AddCronBackgroundService_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        ServiceCollection? serviceCollection = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            serviceCollection!.AddCronBackgroundService<CronHandler>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddCronBackgroundService_WithNullOrWhitespaceCronExpression_ShouldThrowArgumentException(string? cronExpression)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() =>
            serviceCollection.AddCronBackgroundService<CronHandler>(cronExpression!));
    }

    [Fact]
    public void AddCronBackgroundService_WithValidSecondsExpression_ShouldNotThrow()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        const string cronExpressionWithSeconds = "0 0 0 * * *"; // Every day at midnight with seconds

        // Act & Assert - Should not throw
        serviceCollection.AddCronBackgroundService<CronHandler>(cronExpressionWithSeconds, includingSeconds: true);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<CronBackgroundServiceOptions<CronHandler>>>();
        Assert.Equal(cronExpressionWithSeconds, options.Value.CronExpression);
        Assert.True(options.Value.IncludingSeconds);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class CronHandler : ICronHandler
    {
        public Task Handle(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
