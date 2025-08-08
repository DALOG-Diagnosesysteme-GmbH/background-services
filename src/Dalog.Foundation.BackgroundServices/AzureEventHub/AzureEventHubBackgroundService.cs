// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dalog.Foundation.BackgroundServices.AzureEventHub;

internal sealed class AzureEventHubBackgroundService<TMessage, THandler> : BackgroundService
    where THandler : class, IAzureEventHubHandler<TMessage> where TMessage : class, new()
{
    private readonly ILogger<AzureEventHubBackgroundService<TMessage, THandler>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly AzureEventHubBackgroundServiceOptions<TMessage> _options;
    private EventProcessorClient? _processor;

    public AzureEventHubBackgroundService(
        ILogger<AzureEventHubBackgroundService<TMessage, THandler>> logger,
        IServiceProvider serviceProvider,
        IOptions<AzureEventHubBackgroundServiceOptions<TMessage>> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting AEH<{Type}> background service...", typeof(TMessage).Name);

        _options.Validate();

        var storageClient = new BlobContainerClient(_options.StorageConnectionString, _options.StorageContainerName);

        _processor = new EventProcessorClient(
            storageClient,
            _options.ConsumerGroupName,
            _options.EventHubConnectionString,
            new EventProcessorClientOptions
            {
                MaximumWaitTime = _options.MaximumWaitTime,
                PrefetchCount = _options.PrefetchCount,
                LoadBalancingUpdateInterval = _options.LoadBalancingUpdateInterval,
            });

        _processor.ProcessEventAsync += ProcessEventHandler;
        _processor.ProcessErrorAsync += ProcessErrorHandler;
        _processor.PartitionInitializingAsync += ProcessorOnPartitionInitializingAsync;
        _processor.PartitionClosingAsync += ProcessorOnPartitionClosingAsync;
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping AEH<{Type}> background service...", typeof(TMessage).Name);
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            _processor.ProcessEventAsync -= ProcessEventHandler;
            _processor.ProcessErrorAsync -= ProcessErrorHandler;
            _processor.PartitionInitializingAsync -= ProcessorOnPartitionInitializingAsync;
            _processor.PartitionClosingAsync -= ProcessorOnPartitionClosingAsync;
        }

        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Executing AEH<{Type}> background service...", typeof(TMessage).Name);
        try
        {
            if (_processor is not null)
            {
                await _processor.StartProcessingAsync(stoppingToken);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error processing AEH<{Type}>: '{Message}'",
                typeof(TMessage).Name, ex.Message);
        }
        finally
        {
            if (_processor is not null)
            {
                await _processor.StopProcessingAsync(stoppingToken);
            }
        }
    }

    private async Task ProcessEventHandler(ProcessEventArgs args)
    {
        try
        {
            if (!args.HasEvent) return;

            using var activity = _logger.BeginScope(new Dictionary<string, object>
            {
                ["PartitionId"] = args.Partition.PartitionId,
                ["ConsumerGroup"] = args.Partition.ConsumerGroup,
                ["EventHubName"] = args.Partition.EventHubName,
                ["MessageType"] = typeof(TMessage).Name,
                ["MessageHandler"] = typeof(THandler).Name,
            });

            TMessage? messageBody;
            try
            {
                messageBody = JsonSerializer.Deserialize<TMessage>(args.Data.Body.ToArray());
                if (messageBody is null) throw new InvalidOperationException("Deserialized message is null.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize AEH<{Type}> message: '{Message}'", typeof(TMessage).Name, ex.Message);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<THandler>();
            await handler.Handle(messageBody, args.CancellationToken);

            await args.UpdateCheckpointAsync();
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event: {Error}", ex.Message);
        }
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogCritical(
            args.Exception,
            "Error retrieving AEH<{Type}>: '{Message}'",
            typeof(TMessage).Name, args.Exception.Message);
        return Task.CompletedTask;
    }

    private Task ProcessorOnPartitionClosingAsync(PartitionClosingEventArgs arg)
    {
        _logger.LogInformation("AEH<{Type}> partition '{Name}' closing of '{Reason}'", typeof(TMessage).Name, arg.PartitionId, arg.Reason.ToString());
        return Task.CompletedTask;
    }

    private Task ProcessorOnPartitionInitializingAsync(PartitionInitializingEventArgs arg)
    {
        _logger.LogInformation("AEH<{Type}> partition '{Name}' initializing on position '{Position}'", typeof(TMessage).Name, arg.PartitionId, arg.DefaultStartingPosition.ToString());
        return Task.CompletedTask;
    }
}
