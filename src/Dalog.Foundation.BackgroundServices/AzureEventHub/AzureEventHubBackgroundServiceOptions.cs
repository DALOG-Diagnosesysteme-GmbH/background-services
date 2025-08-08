// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;
using Azure.Messaging.EventHubs.Consumer;

namespace Dalog.Foundation.BackgroundServices.AzureEventHub;

/// <summary>
/// Represents configuration options for an Azure Event Hub background service.
/// Provides settings for storage, event hub connection, consumer group, and performance tuning.
/// </summary>
/// <typeparam name="TMessage">
/// The type of the message being processed by the background service. Must be a class with a parameterless constructor.
/// </typeparam>
public class AzureEventHubBackgroundServiceOptions<TMessage> : IBackgroundServiceOptions where TMessage : class, new()
{
    /// <summary>
    /// Gets or sets the number of events to prefetch from the Event Hub for improved performance.
    /// Default is 300.
    /// </summary>
    public int PrefetchCount { get; set; } = 300;

    /// <summary>
    /// Gets or sets the maximum time to wait for events before processing.
    /// Default is 10 seconds.
    /// </summary>
    public TimeSpan MaximumWaitTime { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets the connection string for the Azure Storage account used for checkpointing and load balancing.
    /// </summary>
    public string StorageConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the Azure Storage container used for checkpointing.
    /// </summary>
    public string StorageContainerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection string for the Azure Event Hub.
    /// </summary>
    public string EventHubConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the interval at which load balancing updates are performed.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan LoadBalancingUpdateInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the name of the consumer group to use when connecting to the Event Hub.
    /// Default is the EventHubConsumerClient.DefaultConsumerGroupName.
    /// </summary>
    public string ConsumerGroupName { get; set; } = EventHubConsumerClient.DefaultConsumerGroupName;

    /// <inheritdoc/>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(StorageConnectionString))
            throw new InvalidOperationException("Storage connection string must be provided.");

        if (string.IsNullOrWhiteSpace(StorageContainerName))
            throw new InvalidOperationException("Storage container name must be provided.");

        if (string.IsNullOrWhiteSpace(EventHubConnectionString))
            throw new InvalidOperationException("Event Hub connection string must be provided.");

        if (PrefetchCount < 0)
            throw new InvalidOperationException("Prefetch count must be greater than zero.");

        if (MaximumWaitTime <= TimeSpan.Zero)
            throw new InvalidOperationException("Maximum wait time must be greater than zero.");

        if (LoadBalancingUpdateInterval <= TimeSpan.Zero)
            throw new InvalidOperationException("Load balancing update interval must be greater than zero.");

        if (string.IsNullOrWhiteSpace(ConsumerGroupName))
            throw new InvalidOperationException("Consumer group name must be provided.");
    }
}
