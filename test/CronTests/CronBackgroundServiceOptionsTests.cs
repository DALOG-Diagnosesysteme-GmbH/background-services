// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using Dalog.Foundation.BackgroundServices.Cron;

namespace Dalog.Foundation.BackgroundServicesTests.CronTests;

public class CronBackgroundServiceOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var options = new CronBackgroundServiceOptions<TestCronHandler>();

        // Assert
        Assert.Equal(string.Empty, options.CronExpression);
        Assert.True(options.WaitForRequestCompletion);
        Assert.Equal(33, options.TimeoutInMinutes);
        Assert.False(options.IncludingSeconds);
    }

    [Fact]
    public void SetProperties_ShouldUpdateValues()
    {
        // Arrange
        var options = new CronBackgroundServiceOptions<TestCronHandler>();
        const string expectedCronExpression = "0 0 * * *";
        const bool expectedWaitForCompletion = false;
        const int expectedTimeout = 60;
        const bool expectedIncludingSeconds = true;

        // Act
        options.CronExpression = expectedCronExpression;
        options.WaitForRequestCompletion = expectedWaitForCompletion;
        options.TimeoutInMinutes = expectedTimeout;
        options.IncludingSeconds = expectedIncludingSeconds;

        // Assert
        Assert.Equal(expectedCronExpression, options.CronExpression);
        Assert.Equal(expectedWaitForCompletion, options.WaitForRequestCompletion);
        Assert.Equal(expectedTimeout, options.TimeoutInMinutes);
        Assert.Equal(expectedIncludingSeconds, options.IncludingSeconds);
    }

    [Theory]
    [InlineData("0 * * * *")]
    [InlineData("0 0 * * *")]
    [InlineData("0 0 1 * *")]
    [InlineData("0 0 1 1 *")]
    [InlineData("0 0 1 1 0")]
    public void CronExpression_CanBeSetToValidExpressions(string cronExpression)
    {
        // Arrange
        var options = new CronBackgroundServiceOptions<TestCronHandler>();

        // Act
        options.CronExpression = cronExpression;

        // Assert
        Assert.Equal(cronExpression, options.CronExpression);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void TimeoutInMinutes_CanBeSetToAnyInteger(int timeout)
    {
        // Arrange
        var options = new CronBackgroundServiceOptions<TestCronHandler>();

        // Act
        options.TimeoutInMinutes = timeout;

        // Assert
        Assert.Equal(timeout, options.TimeoutInMinutes);
    }

    // Test helper class
    private sealed class TestCronHandler : ICronHandler
    {
        public Task Handle(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
