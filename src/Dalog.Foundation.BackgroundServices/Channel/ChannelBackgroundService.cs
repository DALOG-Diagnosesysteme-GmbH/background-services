// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dalog.Foundation.BackgroundServices.Channel;

internal sealed class ChannelBackgroundService<TMessage, THandler> : BackgroundService where THandler : class, IChannelHandler<TMessage>
{
    private readonly ILogger<ChannelBackgroundService<TMessage, THandler>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IChannelReader<TMessage> _channelReader;
    private readonly ChannelBackgroundServiceOptions<TMessage> _options;

    public ChannelBackgroundService(
        ILogger<ChannelBackgroundService<TMessage, THandler>> logger,
        IServiceProvider serviceProvider,
        IChannelReader<TMessage> channelReader,
        IOptions<ChannelBackgroundServiceOptions<TMessage>> options)
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
        _logger.LogInformation("Starting Queue<{Type}> background service...", typeof(TMessage).Name);

        _options.Validate();

        return base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Queue<{Type}> background service...", typeof(TMessage).Name);
        if (_options.DrainQueueOnShutdown)
        {
            _logger.LogInformation("Queue<{Type}>: Draining remaining items before shutdown...", typeof(TMessage).Name);
            _channelReader.Complete();
        }
        else
        {
            _logger.LogWarning("Queue<{Type}>: shutdown may drop remaining items. Set DrainQueueOnShutdown = true to avoid this", typeof(TMessage).Name);
        }

        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Executing Queue<{Type}> background service...", typeof(TMessage).Name);
        await foreach (var item in _channelReader.Dequeue(stoppingToken))
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                _logger.LogInformation("Processing Queue<{Type}> item", typeof(TMessage).Name);
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                timeoutCts.CancelAfter(TimeSpan.FromMinutes(_options.TimeoutInMinutes));
                await ProcessItem(item, timeoutCts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error processing Queue<{Type}>: '{Message}'",
                    typeof(TMessage).Name, ex.Message);
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation("Queue<{Type}> processing completed in '{Elapsed}' milliseconds",
                    typeof(TMessage).Name, sw.ElapsedMilliseconds);
            }
        }
    }

    private async Task ProcessItem(TMessage item, CancellationToken cancellationToken)
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["MessageType"] = typeof(TMessage).Name,
            ["MessageHandler"] = typeof(THandler).Name,
        });

        int attempts = 0;
        int maxAttempts = _options.RetryAttempts + 1;
        Exception? lastException = null;

        while (attempts < maxAttempts)
        {
            try
            {
                attempts++;
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                await handler.Handle(item, cancellationToken);
                return; // Success
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (attempts < maxAttempts)
                {
                    _logger.LogWarning(ex, "Attempt {Attempt}/{MaxAttempts} failed for Queue<{Type}>. Retrying in {Delay}ms",
                        attempts, maxAttempts, typeof(TMessage).Name, _options.RetryDelay.TotalMilliseconds);

                    await Task.Delay(_options.RetryDelay, cancellationToken);
                }
            }
        }

        // If we reach here, all attempts failed
        _logger.LogError("All {MaxAttempts} attempts failed for Queue<{Type}>, invoking OnError callback if configured",
            maxAttempts, typeof(TMessage).Name);
        if (_options.OnError is not null)
        {
            try
            {
                await _options.OnError(lastException ?? new Exception($"Failed to process item after {maxAttempts} attempts."), item, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnError callback for Queue<{Type}>: '{Message}'",
                    typeof(TMessage).Name, ex.Message);
            }
        }
    }
}
