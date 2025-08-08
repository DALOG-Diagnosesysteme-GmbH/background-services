// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dalog.Foundation.BackgroundServices.Channel;

/// <summary>
/// Represents the configuration options for a channel-based background service.
/// This class provides settings to control the behavior of the background service,
/// such as timeout, retry attempts, retry delay, error handling, and queue draining on shutdown.
/// </summary>
/// <typeparam name="TQueueItem">
/// The type of the items that will be processed by the background service.
/// </typeparam>
public class ChannelBackgroundServiceOptions<TQueueItem> : IBackgroundServiceOptions
{
    /// <summary>
    /// Gets or sets the timeout value, in minutes, for processing items in the background service.
    /// This value determines the maximum time allowed for processing an item.
    /// </summary>
    public int TimeoutInMinutes { get; set; } = 32;

    /// <summary>
    /// Gets or sets the number of retry attempts for processing an item in case of failure.
    /// This value determines how many times the service will retry processing an item before giving up.
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retry attempts when processing an item fails.
    /// This value determines the wait time before retrying the operation.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets a callback function to handle errors that occur during item processing.
    /// This function is invoked with the exception, the item being processed, and a cancellation token.
    /// </summary>
    public Func<Exception, TQueueItem, CancellationToken, Task>? OnError { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the service should drain the queue on shutdown.
    /// If set to <c>true</c>, the service will process all remaining items in the queue before shutting down.
    /// </summary>
    public bool DrainQueueOnShutdown { get; set; } = true;

    /// <inheritdoc/>
    public void Validate()
    {
        if (TimeoutInMinutes <= 0)
            throw new InvalidOperationException("TimeoutInMinutes must be greater than 0.");

        if (RetryAttempts < 0)
            throw new InvalidOperationException("RetryAttempts must be greater than 0.");

        if (RetryDelay < TimeSpan.Zero)
            throw new InvalidOperationException("RetryDelay must be greater than TimeSpan.Zero.");
    }
}
