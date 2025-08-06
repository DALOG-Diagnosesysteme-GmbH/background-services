// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;
using Dalog.Foundation.BackgroundServices.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering Azure Service Bus background services and their handlers
/// with the dependency injection container.
/// </summary>
public static class AzureServiceBusInjectionExtensions
{
    /// <summary>
    /// Registers an Azure Service Bus background service and its message handler in the dependency injection container.
    /// Allows configuration of service options.
    /// </summary>
    /// <typeparam name="TMessage">
    /// The type of the message to be processed. Must be a class with a parameterless constructor.
    /// </typeparam>
    /// <typeparam name="THandler">
    /// The type of the message handler. Must implement <see cref="IAzureServiceBusHandler{TMessage}"/>.
    /// </typeparam>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to add the services to.
    /// </param>
    /// <param name="configure">
    /// An optional delegate to configure <see cref="AzureServiceBusBackgroundServiceOptions{TMessage}"/>.
    /// </param>
    /// <returns>
    /// The <see cref="IServiceCollection"/> for chaining.
    /// </returns>
    public static IServiceCollection AddAzureServiceBusBackgroundService<TMessage, THandler>(this IServiceCollection services,
        Action<AzureServiceBusBackgroundServiceOptions<TMessage>>? configure = null)
        where THandler : class, IAzureServiceBusHandler<TMessage> where TMessage : class, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register the handler
        services.TryAddScoped<THandler>();

        // Register queue infrastructure
        services.AddHostedService<AzureServiceBusBackgroundService<TMessage, THandler>>();

        // Configure options
        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<AzureServiceBusBackgroundServiceOptions<TMessage>>(_ => { });
        }

        return services;
    }

    /// <summary>
    /// Registers an Azure Service Bus background service and its message handler in the dependency injection container,
    /// using the specified connection string and queue name.
    /// </summary>
    /// <typeparam name="TMessage">
    /// The type of the message to be processed. Must be a class with a parameterless constructor.
    /// </typeparam>
    /// <typeparam name="THandler">
    /// The type of the message handler. Must implement <see cref="IAzureServiceBusHandler{TMessage}"/>.
    /// </typeparam>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to add the services to.
    /// </param>
    /// <param name="connectionString">
    /// The connection string for the Azure Service Bus.
    /// </param>
    /// <param name="queueName">
    /// The name of the Azure Service Bus queue.
    /// </param>
    /// <returns>
    /// The <see cref="IServiceCollection"/> for chaining.
    /// </returns>
    public static IServiceCollection AddAzureServiceBusBackgroundService<TMessage, THandler>(
        this IServiceCollection services,
        string connectionString, string queueName)
        where THandler : class, IAzureServiceBusHandler<TMessage> where TMessage : class, new()
        => services.AddAzureServiceBusBackgroundService<TMessage, THandler>(c =>
        {
            ArgumentNullException.ThrowIfNull(connectionString);
            ArgumentNullException.ThrowIfNull(queueName);

            c.ConnectionString = connectionString;
            c.QueueName = queueName;
        });
}
