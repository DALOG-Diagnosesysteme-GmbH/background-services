// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dalog.Foundation.BackgroundServices.Channel;

/// <summary>
/// Defines a writer for enqueuing items of type <typeparamref name="TQueueItem"/> into a channel.
/// Implementations of this interface are responsible for adding items to the channel for processing by a background service.
/// </summary>
/// <typeparam name="TQueueItem">
/// The type of the items that will be enqueued into the channel.
/// This represents the data type of the queue items handled by the background service.
/// </typeparam>
public interface IChannelWriter<in TQueueItem>
{
    /// <summary>
    /// Enqueues a single item of type <typeparamref name="TQueueItem"/> into the channel asynchronously.
    /// This method adds the item to the channel for processing by the background service.
    /// </summary>
    /// <param name="item">
    /// The item to be enqueued into the channel.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to observe cancellation requests.
    /// The operation should respect this token to allow graceful shutdowns.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> that represents the asynchronous operation of enqueuing the item.
    /// </returns>
    ValueTask Enqueue(TQueueItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enqueues a range of items of type <typeparamref name="TQueueItem"/> into the channel asynchronously.
    /// This method adds multiple items to the channel for processing by the background service.
    /// </summary>
    /// <param name="item">
    /// A collection of items to be enqueued into the channel.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to observe cancellation requests.
    /// The operation should respect this token to allow graceful shutdowns.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> that represents the asynchronous operation of enqueuing the items.
    /// </returns>
    ValueTask EnqueueRange(IEnumerable<TQueueItem> item, CancellationToken cancellationToken = default);
}
