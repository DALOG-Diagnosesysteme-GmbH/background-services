// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System.Threading;
using System.Threading.Tasks;

namespace Dalog.Foundation.BackgroundServices.Channel;

/// <summary>
/// Defines a handler for processing items of type <typeparamref name="TQueueItem"/> in a channel-based background service.
/// Implementations of this interface are responsible for handling individual items from the queue.
/// </summary>
/// <typeparam name="TQueueItem">
/// The type of the items that will be processed by the handler.
/// This represents the data type of the queue items handled by the background service.
/// </typeparam>
public interface IChannelHandler<in TQueueItem>
{
    /// <summary>
    /// Handles a single item from the queue asynchronously.
    /// This method is invoked by the background service to process each item in the channel.
    /// </summary>
    /// <param name="item">
    /// The item of type <typeparamref name="TQueueItem"/> to be processed.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to observe cancellation requests.
    /// The handler should respect this token to allow graceful shutdowns.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation of handling the item.
    /// </returns>
    Task Handle(TQueueItem item, CancellationToken cancellationToken);
}
