// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;

namespace Dalog.Foundation.BackgroundServices.AzureServiceBus;

/// <summary>
/// Represents configuration options for an Azure Service Bus background service.
/// Provides settings for connection string, queue name, and prefetch count.
/// </summary>
/// <typeparam name="TMessage">
/// The type of the message being processed by the background service.
/// Must be a class with a parameterless constructor.
/// </typeparam>
public class AzureServiceBusBackgroundServiceOptions<TMessage> : IBackgroundServiceOptions where TMessage : class, new()
{
    /// <summary>
    /// Gets or sets the connection string for the Azure Service Bus.
    /// This is required to establish a connection to the service bus.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the Azure Service Bus queue.
    /// This specifies the queue from which messages will be processed.
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prefetch count for the Azure Service Bus.
    /// Determines the number of messages to prefetch for improved performance.
    /// Must be zero or greater.
    /// </summary>
    public int PrefetchCount { get; set; } = 0;

    /// <inheritdoc/>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("Azure Service Bus connection string is not configured.");

        if (string.IsNullOrWhiteSpace(QueueName))
            throw new InvalidOperationException("Azure Service Bus queue name is not configured.");

        if (PrefetchCount < 0)
            throw new InvalidOperationException("Azure Service Bus prefetch count must be zero or greater.");
    }
}
