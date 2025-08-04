# Background Services

A .NET library providing robust background service implementations for channel-based message processing and cron-scheduled task execution.

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

### Test Coverage

- ✅ 52 unit tests covering all public APIs
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

## License

Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved