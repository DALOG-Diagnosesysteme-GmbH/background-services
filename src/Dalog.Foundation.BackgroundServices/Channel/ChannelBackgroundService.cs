// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dalog.Foundation.BackgroundServices.Channel;

internal sealed class ChannelBackgroundService<TQueueItem> : BackgroundService
{
    private readonly ILogger<ChannelBackgroundService<TQueueItem>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IChannelReader<TQueueItem> _channelReader;
    private readonly ChannelBackgroundServiceOptions<TQueueItem> _options;

    public ChannelBackgroundService(
        ILogger<ChannelBackgroundService<TQueueItem>> logger,
        IServiceProvider serviceProvider,
        IChannelReader<TQueueItem> channelReader,
        IOptions<ChannelBackgroundServiceOptions<TQueueItem>> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(channelReader);
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;
        _serviceProvider = serviceProvider;
        _channelReader = channelReader;
        _options = options.Value;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Queue<{Type}> background service...", typeof(TQueueItem).Name);
        return base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Queue<{Type}> background service...", typeof(TQueueItem).Name);
        if (_options.DrainQueueOnShutdown)
        {
            _logger.LogInformation("Queue<{Type}>: Draining remaining items before shutdown...", typeof(TQueueItem).Name);
            _channelReader.Complete();
        }
        else
        {
            _logger.LogWarning("Queue<{Type}>: shutdown may drop remaining items. Set DrainQueueOnShutdown = true to avoid this", typeof(TQueueItem).Name);
        }
        
        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Executing Queue<{Type}> background service...", typeof(TQueueItem).Name);
        await foreach(var item in _channelReader.Dequeue(stoppingToken))
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                _logger.LogInformation("Processing Queue<{Type}> item", typeof(TQueueItem).Name);
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                timeoutCts.CancelAfter(TimeSpan.FromMinutes(_options.TimeoutInMinutes));
                await ProcessItem(item, timeoutCts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error processing Queue<{Type}>: '{Message}'",
                    typeof(TQueueItem).Name, ex.Message);
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation("Queue<{Type}> processing completed in '{Elapsed}' milliseconds",
                    typeof(TQueueItem).Name, sw.ElapsedMilliseconds);
            }
        }
    }

    private async Task ProcessItem(TQueueItem item, CancellationToken cancellationToken)
    {
        int attempts = 0;
        int maxAttempts = _options.RetryAttempts + 1;
        Exception? lastException = null;

        while (attempts < maxAttempts)
        {
            try
            {
                attempts++;
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IChannelHandler<TQueueItem>>();
                await handler.Handle(item, cancellationToken);
                return; // Success
            }
            catch (Exception ex)
            {
                lastException = ex;
                
                if (attempts < maxAttempts)
                {
                    _logger.LogWarning(ex, "Attempt {Attempt}/{MaxAttempts} failed for Queue<{Type}>. Retrying in {Delay}ms",
                        attempts, maxAttempts, typeof(TQueueItem).Name, _options.RetryDelay.TotalMilliseconds);

                    await Task.Delay(_options.RetryDelay, cancellationToken);
                }
            }
        }

        // If we reach here, all attempts failed
        _logger.LogError("All {MaxAttempts} attempts failed for Queue<{Type}>, invoking OnError callback if configured",
            maxAttempts, typeof(TQueueItem).Name);
        if (_options.OnError is not null)
        {
            try
            {
                await _options.OnError(lastException ?? new Exception($"Failed to process item after {maxAttempts} attempts."), item, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnError callback for Queue<{Type}>: '{Message}'",
                    typeof(TQueueItem).Name, ex.Message);
            }
        }
    }
}
