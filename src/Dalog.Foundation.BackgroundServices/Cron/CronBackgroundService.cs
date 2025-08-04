// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCrontab;

namespace Dalog.Foundation.BackgroundServices.Cron;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class CronBackgroundService<THandler> : BackgroundService where THandler : class, ICronHandler
{
    private readonly ILogger<CronBackgroundService<THandler>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CronBackgroundServiceOptions<THandler> _options;
    private readonly CrontabSchedule _schedule;

    public CronBackgroundService(
        ILogger<CronBackgroundService<THandler>> logger,
        IServiceProvider serviceProvider,
        IOptions<CronBackgroundServiceOptions<THandler>> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _schedule = CrontabSchedule.Parse(options.Value.CronExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = _options.IncludingSeconds });
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Cron<{Type}> background service...", typeof(THandler).Name);
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Cron<{Type}> background service...", typeof(THandler).Name);
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Executing Cron<{Type}> background service...", typeof(THandler).Name);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nextOccurrence = _schedule.GetNextOccurrence(DateTime.UtcNow);
                var delay = nextOccurrence - DateTime.UtcNow;
                if (delay <= TimeSpan.Zero) delay = TimeSpan.FromSeconds(1);
                _logger.LogInformation("Next occurrence of the Cron<{Type}> is in '{Delay}' seconds...",
                    typeof(THandler).Name, delay.TotalSeconds);
                await Task.Delay(delay, stoppingToken);

                if (_options.WaitForRequestCompletion)
                {
                    await RunRequest(stoppingToken);
                }
                else
                {
                    // Run the timer action in the background, do not await
                    _ = Task.Run(() => RunRequest(stoppingToken), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error executing Cron<{Type}> background service: '{Message}'",
                    typeof(THandler).Name, ex.Message);
            }
        }
    }

    private async Task RunRequest(CancellationToken cancellationToken)
    {
        var sw = new Stopwatch();
        sw.Start();
        try
        {
            _logger.LogInformation("Processing Cron<{Type}> item", typeof(THandler).Name);
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<THandler>();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromMinutes(_options.TimeoutInMinutes));
            await handler.Handle(timeoutCts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error timer elapsed Cron<{Type}> Timer background service: '{Message}'",
                typeof(THandler).Name, ex.Message);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("Cron<{Type}> processing completed in '{Elapsed}' milliseconds",
                typeof(THandler).Name, sw.ElapsedMilliseconds);
        }
    }
}
