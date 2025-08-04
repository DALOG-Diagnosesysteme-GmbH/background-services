// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;
using Dalog.Foundation.BackgroundServices.Cron;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCrontab;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering cron-based background services in the dependency injection container.
/// This class simplifies the setup and configuration of cron background services and their associated handlers.
/// </summary>
public static class CronInjectionExtensions
{
    /// <summary>
    /// Registers a cron-based background service with the specified handler type and optional configuration action.
    /// This method sets up the necessary infrastructure for executing tasks based on a cron schedule.
    /// </summary>
    /// <typeparam name="THandler">
    /// The type of the handler that processes the cron jobs.
    /// The handler must implement the <see cref="ICronHandler"/> interface.
    /// </typeparam>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the cron background service and its dependencies will be added.
    /// </param>
    /// <param name="configure">
    /// An optional action to configure the <see cref="CronBackgroundServiceOptions{THandler}"/>.
    /// If not provided, default options will be used.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> with the registered cron background service and its dependencies.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="services"/> parameter is null.
    /// </exception>
    public static IServiceCollection AddCronBackgroundService<THandler>(
        this IServiceCollection services,
        Action<CronBackgroundServiceOptions<THandler>>? configure = null)
        where THandler : class, ICronHandler
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<THandler>();
        services.AddHostedService<CronBackgroundService<THandler>>();

        if (configure is not null)
        {
            var tempOptions = new CronBackgroundServiceOptions<THandler>();
            configure(tempOptions);
            _ = CrontabSchedule.Parse(tempOptions.CronExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = tempOptions.IncludingSeconds });

            services.Configure(configure);
        }
        else
        {
            services.Configure<CronBackgroundServiceOptions<THandler>>(_ => { });
        }

        return services;
    }

    /// <summary>
    /// Registers a cron-based background service with the specified handler type and cron expression.
    /// This method is a convenience overload that allows specifying the cron expression directly.
    /// </summary>
    /// <typeparam name="THandler">
    /// The type of the handler that processes the cron jobs.
    /// The handler must implement the <see cref="ICronHandler"/> interface.
    /// </typeparam>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the cron background service and its dependencies will be added.
    /// </param>
    /// <param name="cronExpression">
    /// The cron expression that defines the schedule for the background service.
    /// This value is used to configure the <see cref="CronBackgroundServiceOptions{THandler}.CronExpression"/>.
    /// </param>
    /// <param name="includingSeconds">A flag indicating whether the cron expression includes seconds.</param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> with the registered cron background service and its dependencies.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="services"/> parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="cronExpression"/> parameter is null or whitespace.
    /// </exception>
    public static IServiceCollection AddCronBackgroundService<THandler>(
        this IServiceCollection services,
        string cronExpression,
        bool includingSeconds = false)
        where THandler : class, ICronHandler
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(cronExpression);

        return services.AddCronBackgroundService<THandler>(c =>
        {
            c.CronExpression = cronExpression;
            c.IncludingSeconds = includingSeconds;
        });
    }
}
