# Background Services

A .NET library providing robust background service implementations for channel-based message processing, cron-scheduled task execution, and Azure Service Bus message processing.

## Features

### Channel Background Services

- **Queue-based processing**: Process items from an unbounded channel
- **Retry logic**: Configurable retry attempts with delays
- **Error handling**: Custom error callbacks for failed items
- **Graceful shutdown**: Option to drain queue on service shutdown
- **Timeout management**: Configurable timeouts for item processing

### Cron Background Services

- **Cron scheduling**: Support for cron expressions with optional seconds
- **Flexible execution**: Choice between waiting for completion or fire-and-forget
- **Timeout support**: Configurable timeouts for scheduled tasks

### Azure Service Bus Background Services

- **Message processing**: Process messages from Azure Service Bus queues
- **JSON serialization**: Automatic JSON deserialization of message bodies
- **Application properties**: Access to message application properties
- **Prefetch support**: Configurable prefetch count for performance optimization
- **Auto-complete**: Automatic message completion on successful processing

## Installation

```bash
dotnet add package Dalog.Foundation.BackgroundServices
```

## Usage

### Channel Background Service

```csharp
// Define your handler
public class EmailHandler : IChannelHandler<EmailMessage>
{
    public async Task Handle(EmailMessage item, CancellationToken cancellationToken)
    {
        // Process the email message
        await SendEmailAsync(item);
    }
}

// Register in DI container
services.AddChannelBackgroundService<EmailMessage, EmailHandler>(options =>
{
    options.TimeoutInMinutes = 5;
    options.RetryAttempts = 3;
    options.RetryDelay = TimeSpan.FromSeconds(30);
    options.DrainQueueOnShutdown = true;
});

// Enqueue items for processing
var writer = serviceProvider.GetRequiredService<IChannelWriter<EmailMessage>>();
await writer.Enqueue(new EmailMessage("test@example.com", "Hello World"));
```

### Cron Background Service

```csharp
// Define your handler
public class MaintenanceHandler : ICronHandler
{
    public async Task Handle(CancellationToken cancellationToken)
    {
        // Perform maintenance tasks
        await RunMaintenanceAsync();
    }
}

// Register in DI container
services.AddCronBackgroundService<MaintenanceHandler>(options =>
{
    options.CronExpression = "0 2 * * *"; // Run daily at 2 AM
    options.TimeoutInMinutes = 30;
    options.WaitForRequestCompletion = true;
});
```

### Azure Service Bus Background Service

```csharp
// Define your message type
public class OrderMessage
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
}

// Define your handler
public class OrderHandler : IAzureServiceBusHandler<OrderMessage>
{
    public async Task Handle(OrderMessage message, IReadOnlyDictionary<string, object> applicationProperties, 
        CancellationToken cancellationToken)
    {
        // Process the order message
        await ProcessOrderAsync(message);
    }
}

// Register in DI container
services.AddAzureServiceBusBackgroundService<OrderMessage, OrderHandler>(options =>
{
    options.ConnectionString = "Endpoint=sb://myservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=...";
    options.QueueName = "orders";
    options.PrefetchCount = 10;
});

// Or use the simplified overload
services.AddAzureServiceBusBackgroundService<OrderMessage, OrderHandler>(
    connectionString: "Endpoint=sb://myservicebus.servicebus.windows.net/;...",
    queueName: "orders");
```

## Testing

The library includes comprehensive unit tests covering:

### Channel Services

- **Injection Tests**: Service registration and dependency injection
- **Service Tests**: Core channel functionality (enqueue, dequeue, completion)
- **Options Tests**: Configuration validation and default values

### Cron Services

- **Injection Tests**: Service registration with various cron expressions
- **Options Tests**: Configuration validation and cron expression parsing
- **Validation Tests**: Invalid cron expression handling

### Azure Service Bus Services

- **Injection Tests**: Service registration and dependency injection
- **Options Tests**: Configuration validation and connection string handling

### Test Coverage

- ✅ 81 unit tests covering all public APIs
- ✅ Dependency injection validation
- ✅ Configuration options testing
- ✅ Error handling scenarios
- ✅ Cancellation token support
- ✅ Invalid input validation

Run tests:

```bash
dotnet test
```

## Configuration Options

### ChannelBackgroundServiceOptions

- `TimeoutInMinutes`: Maximum processing time per item (default: 32)
- `RetryAttempts`: Number of retry attempts on failure (default: 3)
- `RetryDelay`: Delay between retries (default: 30 seconds)
- `DrainQueueOnShutdown`: Whether to process remaining items on shutdown (default: true)
- `OnError`: Custom error handling callback

### CronBackgroundServiceOptions

- `CronExpression`: Cron schedule expression
- `TimeoutInMinutes`: Maximum execution time (default: 33)
- `WaitForRequestCompletion`: Wait for previous execution to complete (default: true)
- `IncludingSeconds`: Whether cron expression includes seconds (default: false)

### AzureServiceBusBackgroundServiceOptions

- `ConnectionString`: Azure Service Bus connection string
- `QueueName`: Name of the Azure Service Bus queue
- `PrefetchCount`: Number of messages to prefetch (default: 0)

## License

Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved