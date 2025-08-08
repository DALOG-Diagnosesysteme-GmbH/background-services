// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;
using NCrontab;

namespace Dalog.Foundation.BackgroundServices.Cron;

/// <summary>
/// Represents the configuration options for a cron-based background service.
/// This class provides settings to control the behavior of the background service,
/// such as the cron expression, timeout, and whether to wait for request completion.
/// </summary>
/// <typeparam name="THandler">
/// The type of the handler that processes the cron jobs.
/// The handler must implement the <see cref="ICronHandler"/> interface.
/// </typeparam>
public class CronBackgroundServiceOptions<THandler> : IBackgroundServiceOptions where THandler : class, ICronHandler
{
    /// <summary>
    /// Gets or sets the cron expression that defines the schedule for the background service.
    /// The cron expression determines when the service will execute its tasks.
    /// </summary>
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the service should wait for the completion of requests.
    /// If set to <c>true</c>, the service will wait for all requests to complete before proceeding.
    /// </summary>
    public bool WaitForRequestCompletion { get; set; } = true;

    /// <summary>
    /// Gets or sets the timeout value, in minutes, for processing cron jobs.
    /// This value determines the maximum time allowed for a job to complete.
    /// </summary>
    public int TimeoutInMinutes { get; set; } = 33;

    /// <summary>
    /// Gets or sets a value indicating whether the cron expression includes seconds.
    /// If set to <c>true</c>, the cron expression will account for seconds in its schedule.
    /// </summary>
    public bool IncludingSeconds { get; set; }

    /// <inheritdoc/>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(CronExpression))
            throw new InvalidOperationException("CronExpression must not be null or empty.");

        _ = CrontabSchedule.Parse(CronExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = IncludingSeconds });

        if (TimeoutInMinutes <= 0)
            throw new InvalidOperationException("TimeoutInMinutes must be greater than 0.");
    }
}
