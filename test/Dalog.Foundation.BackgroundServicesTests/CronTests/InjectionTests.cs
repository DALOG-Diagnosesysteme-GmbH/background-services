// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using Dalog.Foundation.BackgroundServices.Cron;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NCrontab;

namespace Dalog.Foundation.BackgroundServicesTests.CronTests;

public class InjectionTests
{
    [Fact]
    public void ItShouldInjectServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddCronBackgroundService<CronHandler>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetRequiredService<CronHandler>());
        Assert.NotNull(serviceProvider.GetRequiredService<IOptions<CronBackgroundServiceOptions<CronHandler>>>());
    }

    [Fact]
    public void ItShouldThrowOnWrongCronExpression()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act & Assert
        Assert.Throws<CrontabException>(() => serviceCollection.AddCronBackgroundService<CronHandler>("a * * * *"));
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class CronHandler : ICronHandler
    {
        public Task Handle(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
