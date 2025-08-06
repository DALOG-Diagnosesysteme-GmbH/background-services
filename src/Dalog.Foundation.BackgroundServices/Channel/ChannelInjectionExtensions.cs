// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;
using Dalog.Foundation.BackgroundServices.Channel;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering channel-based background services in the dependency injection container.
/// This class simplifies the setup and configuration of channel background services and their associated handlers.
/// </summary>
public static class ChannelInjectionExtensions
{
    /// <summary>
    /// Registers a channel background service with the specified queue item type and handler.
    /// This method sets up the necessary infrastructure for processing items from a channel in the background.
    /// It registers the handler, channel writer, and channel reader, and configures the hosted service.
    /// </summary>
    /// <typeparam name="TQueueItem">
    /// The type of the items that will be processed by the channel.
    /// This represents the data type of the queue items handled by the background service.
    /// </typeparam>
    /// <typeparam name="THandler">
    /// The type of the handler that processes the queue items.
    /// The handler must implement the <see cref="IChannelHandler{TQueueItem}"/> interface.
    /// </typeparam>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the channel background service and its dependencies will be added.
    /// </param>
    /// <param name="configure">
    /// An optional action to configure the <see cref="ChannelBackgroundServiceOptions{TQueueItem}"/>.
    /// If not provided, default options will be used.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> with the registered channel background service and its dependencies.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="services"/> parameter is null.
    /// </exception>
    public static IServiceCollection AddChannelBackgroundService<TQueueItem, THandler>(this IServiceCollection services,
        Action<ChannelBackgroundServiceOptions<TQueueItem>>? configure = null)
        where THandler : class, IChannelHandler<TQueueItem>
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register the handler
        services.TryAddScoped<THandler>();

        // Register queue infrastructure
        var channel = new ChannelService<TQueueItem>();
        services.TryAddSingleton<IChannelWriter<TQueueItem>>(channel);
        services.TryAddSingleton<IChannelReader<TQueueItem>>(channel);

        services.AddHostedService<ChannelBackgroundService<TQueueItem, THandler>>();


        // Configure options
        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<ChannelBackgroundServiceOptions<TQueueItem>>(_ => { });
        }

        return services;
    }

    /// <summary>
    /// Registers a channel background service with the specified queue item type and handler,
    /// and configures it with a timeout value in minutes.
    /// This method is a convenience overload that allows specifying the timeout directly.
    /// </summary>
    /// <typeparam name="TQueueItem">
    /// The type of the items that will be processed by the channel.
    /// This represents the data type of the queue items handled by the background service.
    /// </typeparam>
    /// <typeparam name="THandler">
    /// The type of the handler that processes the queue items.
    /// The handler must implement the <see cref="IChannelHandler{TQueueItem}"/> interface.
    /// </typeparam>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the channel background service and its dependencies will be added.
    /// </param>
    /// <param name="timeoutInMinutes">
    /// The timeout value, in minutes, for processing items in the channel.
    /// This value is used to configure the <see cref="ChannelBackgroundServiceOptions{TQueueItem}.TimeoutInMinutes"/>.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> with the registered channel background service and its dependencies.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="services"/> parameter is null.
    /// </exception>
    public static IServiceCollection AddChannelBackgroundService<TQueueItem, THandler>(
        this IServiceCollection services,
        int timeoutInMinutes)
        where THandler : class, IChannelHandler<TQueueItem>
        => services.AddChannelBackgroundService<TQueueItem, THandler>(options =>
        {
            options.TimeoutInMinutes = timeoutInMinutes;
        });
}
