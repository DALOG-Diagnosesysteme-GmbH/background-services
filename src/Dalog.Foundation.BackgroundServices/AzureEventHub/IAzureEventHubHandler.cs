// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System.Threading;
using System.Threading.Tasks;

namespace Dalog.Foundation.BackgroundServices.AzureEventHub;

/// <summary>
/// Defines a handler for processing messages received from Azure Event Hub.
/// </summary>
/// <typeparam name="TMessage">
/// The type of the message to be processed. Must be a class with a parameterless constructor.
/// </typeparam>
public interface IAzureEventHubHandler<in TMessage> where TMessage : class, new()
{
    /// <summary>
    /// Handles a message received from Azure Event Hub.
    /// </summary>
    /// <param name="message">
    /// The message to process. Cannot be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous message handling operation.
    /// </returns>
    Task Handle(TMessage message, CancellationToken cancellationToken);
}
