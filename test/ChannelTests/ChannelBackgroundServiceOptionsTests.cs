// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using Dalog.Foundation.BackgroundServices.Channel;

namespace Dalog.Foundation.BackgroundServicesTests.ChannelTests;

public class ChannelBackgroundServiceOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var options = new ChannelBackgroundServiceOptions<string>();

        // Assert
        Assert.Equal(32, options.TimeoutInMinutes);
        Assert.Equal(3, options.RetryAttempts);
        Assert.Equal(TimeSpan.FromSeconds(30), options.RetryDelay);
        Assert.True(options.DrainQueueOnShutdown);
        Assert.Null(options.OnError);
    }

    [Fact]
    public void SetProperties_ShouldUpdateValues()
    {
        // Arrange
        var options = new ChannelBackgroundServiceOptions<string>();
        const int expectedTimeout = 60;
        const int expectedRetryAttempts = 5;
        var expectedRetryDelay = TimeSpan.FromMinutes(1);
        const bool expectedDrainQueue = false;
        
        Func<Exception, string, CancellationToken, Task> expectedOnError = 
            (ex, item, ct) => Task.CompletedTask;

        // Act
        options.TimeoutInMinutes = expectedTimeout;
        options.RetryAttempts = expectedRetryAttempts;
        options.RetryDelay = expectedRetryDelay;
        options.DrainQueueOnShutdown = expectedDrainQueue;
        options.OnError = expectedOnError;

        // Assert
        Assert.Equal(expectedTimeout, options.TimeoutInMinutes);
        Assert.Equal(expectedRetryAttempts, options.RetryAttempts);
        Assert.Equal(expectedRetryDelay, options.RetryDelay);
        Assert.Equal(expectedDrainQueue, options.DrainQueueOnShutdown);
        Assert.Equal(expectedOnError, options.OnError);
    }

    [Fact]
    public void OnError_CanBeSetToNull()
    {
        // Arrange
        var options = new ChannelBackgroundServiceOptions<string>
        {
            OnError = (ex, item, ct) => Task.CompletedTask
        };

        // Act
        options.OnError = null;

        // Assert
        Assert.Null(options.OnError);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void TimeoutInMinutes_CanBeSetToZeroOrNegative(int timeout)
    {
        // Arrange
        var options = new ChannelBackgroundServiceOptions<string>();

        // Act
        options.TimeoutInMinutes = timeout;

        // Assert
        Assert.Equal(timeout, options.TimeoutInMinutes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void RetryAttempts_CanBeSetToZeroOrNegative(int retryAttempts)
    {
        // Arrange
        var options = new ChannelBackgroundServiceOptions<string>();

        // Act
        options.RetryAttempts = retryAttempts;

        // Assert
        Assert.Equal(retryAttempts, options.RetryAttempts);
    }
}
