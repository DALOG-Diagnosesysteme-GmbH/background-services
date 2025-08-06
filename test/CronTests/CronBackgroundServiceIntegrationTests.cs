// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using Dalog.Foundation.BackgroundServices.Cron;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dalog.Foundation.BackgroundServicesTests.CronTests;

public class CronBackgroundServiceIntegrationTests
{
    [Fact]
    public async Task CronBackgroundService_ShouldStartAndStopCorrectly()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddCronBackgroundService<TestCronHandler>(options =>
        {
            options.CronExpression = "* * * * *"; // Every minute (won't actually execute in this short test)
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var hostedService = serviceProvider.GetServices<IHostedService>()
            .First(s => s.GetType().Name.Contains("CronBackgroundService"));

        // Act & Assert - Should not throw
        await hostedService.StartAsync(CancellationToken.None);
        await Task.Delay(50); // Short delay
        await hostedService.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CronBackgroundService_WithVeryFrequentSchedule_ShouldExecuteHandler()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        // Register handler explicitly
        serviceCollection.AddSingleton<CountingCronHandler>();
        serviceCollection.AddCronBackgroundService<CountingCronHandler>(options =>
        {
            options.CronExpression = "* * * * * *"; // Every second
            options.IncludingSeconds = true;
            options.TimeoutInMinutes = 1;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var hostedService = serviceProvider.GetServices<IHostedService>()
            .First(s => s.GetType().Name.Contains("CronBackgroundService"));
        var handler = serviceProvider.GetRequiredService<CountingCronHandler>();

        // Act
        await hostedService.StartAsync(CancellationToken.None);

        // Wait for at least one execution (but not too long to avoid test timeout)
        await Task.Delay(2000); // 2 seconds should be enough for at least one execution

        await hostedService.StopAsync(CancellationToken.None);

        // Assert
        Assert.True(handler.ExecutionCount > 0, "Handler should have been executed at least once");
    }

    [Fact]
    public async Task CronBackgroundService_WithWaitForCompletion_ShouldWaitForHandlerCompletion()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        // Register handler explicitly
        serviceCollection.AddSingleton<SlowCronHandler>();
        serviceCollection.AddCronBackgroundService<SlowCronHandler>(options =>
        {
            options.CronExpression = "* * * * * *"; // Every second
            options.IncludingSeconds = true;
            options.WaitForRequestCompletion = true;
            options.TimeoutInMinutes = 1;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var hostedService = serviceProvider.GetServices<IHostedService>()
            .First(s => s.GetType().Name.Contains("CronBackgroundService"));
        var handler = serviceProvider.GetRequiredService<SlowCronHandler>();

        // Act
        await hostedService.StartAsync(CancellationToken.None);

        // Wait for at least one execution to start
        await Task.Delay(1200); // Wait longer than one second to allow execution to start

        // Check that only one execution has started (due to WaitForRequestCompletion=true)
        var executionCount = handler.ExecutionCount;

        await hostedService.StopAsync(CancellationToken.None);

        // Assert
        Assert.True(executionCount > 0, "At least one execution should have started");
        // With WaitForRequestCompletion=true and slow handler, we shouldn't see many concurrent executions
        Assert.True(executionCount <= 2, "Should not have too many concurrent executions with WaitForRequestCompletion=true");
    }

    [Fact]
    public async Task CronBackgroundService_WithoutWaitForCompletion_ShouldAllowConcurrentExecutions()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        // Register handler explicitly
        serviceCollection.AddSingleton<CountingCronHandler>(); // Use fast handler for this test
        serviceCollection.AddCronBackgroundService<CountingCronHandler>(options =>
        {
            options.CronExpression = "* * * * * *"; // Every second
            options.IncludingSeconds = true;
            options.WaitForRequestCompletion = false;
            options.TimeoutInMinutes = 1;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var hostedService = serviceProvider.GetServices<IHostedService>()
            .First(s => s.GetType().Name.Contains("CronBackgroundService"));
        var handler = serviceProvider.GetRequiredService<CountingCronHandler>();

        // Act
        await hostedService.StartAsync(CancellationToken.None);

        // Wait for multiple potential executions
        await Task.Delay(2500); // 2.5 seconds should allow multiple executions to start

        await hostedService.StopAsync(CancellationToken.None);

        // Assert
        // With WaitForRequestCompletion=false, multiple executions should be able to start
        Assert.True(handler.ExecutionCount >= 2, "Multiple executions should have occurred with WaitForRequestCompletion=false");
    }

    // Test helper classes
    private sealed class TestCronHandler : ICronHandler
    {
        public int ExecutionCount { get; private set; }

        public Task Handle(CancellationToken cancellationToken)
        {
            ExecutionCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class CountingCronHandler : ICronHandler
    {
        private int _executionCount;
        public int ExecutionCount => _executionCount;

        public Task Handle(CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _executionCount);
            return Task.CompletedTask;
        }
    }

    private sealed class SlowCronHandler : ICronHandler
    {
        private int _executionCount;
        public int ExecutionCount => _executionCount;

        public async Task Handle(CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _executionCount);
            // Simulate slow operation
            await Task.Delay(1500, cancellationToken);
        }
    }
}
