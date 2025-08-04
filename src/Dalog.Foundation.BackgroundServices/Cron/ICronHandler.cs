// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System.Threading;
using System.Threading.Tasks;

namespace Dalog.Foundation.BackgroundServices.Cron;

/// <summary>
/// Defines a handler for executing tasks based on a cron schedule in a background service.
/// Implementations of this interface are responsible for performing the scheduled tasks.
/// </summary>
public interface ICronHandler
{
    /// <summary>
    /// Executes the scheduled task asynchronously based on the cron schedule.
    /// This method is invoked by the background service to perform the task.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to observe cancellation requests.
    /// The handler should respect this token to allow graceful shutdowns.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation of executing the scheduled task.
    /// </returns>
    Task Handle(CancellationToken cancellationToken);
}
