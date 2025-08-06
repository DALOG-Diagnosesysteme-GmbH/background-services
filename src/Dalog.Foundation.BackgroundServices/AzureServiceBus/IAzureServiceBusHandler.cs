// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dalog.Foundation.BackgroundServices.AzureServiceBus;

/// <summary>
/// Defines a handler for processing messages received from Azure Service Bus.
/// Provides a method to handle messages along with their application properties and cancellation token.
/// </summary>
/// <typeparam name="TMessage">
/// The type of the message being processed. Must be a class with a parameterless constructor.
/// </typeparam>
public interface IAzureServiceBusHandler<in TMessage> where TMessage : class, new()
{
    /// <summary>
    /// Handles a message received from Azure Service Bus.
    /// Processes the message, its associated application properties, and respects the provided cancellation token.
    /// </summary>
    /// <param name="message">
    /// The message received from Azure Service Bus. Cannot be null.
    /// </param>
    /// <param name="applicationProperties">
    /// A read-only dictionary containing application-specific properties associated with the message.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests, allowing the operation to be canceled gracefully.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation of handling the message.
    /// </returns>
    Task Handle(TMessage message, IReadOnlyDictionary<string, object> applicationProperties,
        CancellationToken cancellationToken);
}
