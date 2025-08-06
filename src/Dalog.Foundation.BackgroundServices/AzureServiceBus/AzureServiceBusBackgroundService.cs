// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dalog.Foundation.BackgroundServices.AzureServiceBus;

internal sealed class AzureServiceBusBackgroundService<TMessage, THandler> : BackgroundService, IAsyncDisposable
    where THandler : class, IAzureServiceBusHandler<TMessage> where TMessage : class, new()
{
    private readonly ILogger<AzureServiceBusBackgroundService<TMessage, THandler>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly AzureServiceBusBackgroundServiceOptions<TMessage> _options;
    private ServiceBusProcessor? _processor;
    private ServiceBusClient? _serviceBusClient;

    public AzureServiceBusBackgroundService(
        ILogger<AzureServiceBusBackgroundService<TMessage, THandler>> logger,
        IServiceProvider serviceProvider,
        IOptions<AzureServiceBusBackgroundServiceOptions<TMessage>> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (_processor is not null)
        {
            await _processor.DisposeAsync();
        }

        if (_serviceBusClient is not null)
        {
            await _serviceBusClient.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting ASB<{Type}> background service...", typeof(TMessage).Name);

        _options.Validate();

        _serviceBusClient = new ServiceBusClient(_options.ConnectionString);
        var options = new ServiceBusProcessorOptions
        {
            PrefetchCount = _options.PrefetchCount,
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            AutoCompleteMessages = true,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10),
            MaxConcurrentCalls = 1
        };
        _processor = _serviceBusClient.CreateProcessor(_options.QueueName, options);
        _processor.ProcessMessageAsync += HandleMessage;
        _processor.ProcessErrorAsync += HandleError;
        return base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping ASB<{Type}> background service...", typeof(TMessage).Name);
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            _processor.ProcessMessageAsync -= HandleMessage;
            _processor.ProcessErrorAsync -= HandleError;
            await _processor.CloseAsync(cancellationToken);
            await _processor.DisposeAsync();
        }

        if (_serviceBusClient is not null)
        {
            await _serviceBusClient.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Executing ASB<{Type}> background service...", typeof(TMessage).Name);
            if (_processor is not null)
            {
                await _processor.StartProcessingAsync(stoppingToken);

                // Keep the service running until cancellation is requested
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error processing ASB<{Type}>: '{Message}'",
                typeof(TMessage).Name, ex.Message);
        }
    }

    private Task HandleError(ProcessErrorEventArgs e)
    {
        _logger.LogCritical(
            e.Exception,
            "Error retrieving  ASB<{Type}>: '{Message}'",
            typeof(TMessage).Name, e.Exception.Message);
        return Task.CompletedTask;
    }

    private async Task HandleMessage(ProcessMessageEventArgs e)
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["MessageId"] = e.Message.MessageId,
            ["CorrelationId"] = e.Message.CorrelationId,
            ["MessageType"] = typeof(TMessage).Name,
            ["MessageHandler"] = typeof(THandler).Name,
            ["QueueName"] = _options.QueueName,
            ["SessionId"] = e.Message.SessionId,
            ["EnqueuedTime"] = e.Message.EnqueuedTime
        });

        TMessage? messageBody;
        try
        {
            messageBody = e.Message.Body.ToObjectFromJson<TMessage>();
            if (messageBody is null) throw new InvalidOperationException("Deserialized message is null.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize ASB<{Type}> message: '{Message}'", typeof(TMessage).Name, ex.Message);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<THandler>();
        await handler.Handle(messageBody, e.Message.ApplicationProperties, e.CancellationToken);
    }
}
