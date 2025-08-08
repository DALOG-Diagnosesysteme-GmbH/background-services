// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;
using Dalog.Foundation.BackgroundServices.AzureEventHub;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering Azure Event Hub background services and their handlers
/// with the dependency injection container.
/// </summary>
public static class AzureEventHubInjectionExtensions
{
    /// <summary>
    /// Registers an Azure Event Hub background service and its message handler in the dependency injection container.
    /// Allows configuration of service options.
    /// </summary>
    /// <typeparam name="TMessage">
    /// The type of the message to be processed. Must be a class with a parameterless constructor.
    /// </typeparam>
    /// <typeparam name="THandler">
    /// The type of the message handler. Must implement <see cref="IAzureEventHubHandler{TMessage}"/>.
    /// </typeparam>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to add the services to.
    /// </param>
    /// <param name="configure">
    /// An optional delegate to configure <see cref="AzureEventHubBackgroundServiceOptions{TMessage}"/>.
    /// </param>
    /// <returns>
    /// The <see cref="IServiceCollection"/> for chaining.
    /// </returns>
    public static IServiceCollection AddAzureEventHubBackgroundService<TMessage, THandler>(this IServiceCollection services,
        Action<AzureEventHubBackgroundServiceOptions<TMessage>>? configure = null)
        where THandler : class, IAzureEventHubHandler<TMessage> where TMessage : class, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register the handler
        services.TryAddScoped<THandler>();

        // Register queue infrastructure
        services.AddHostedService<AzureEventHubBackgroundService<TMessage, THandler>>();

        // Configure options
        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<AzureEventHubBackgroundServiceOptions<TMessage>>(_ => { });
        }

        return services;
    }
}
