// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using Dalog.Foundation.BackgroundServices.Channel;

namespace Dalog.Foundation.BackgroundServicesTests.ChannelTests;

public class ChannelServiceTests
{
    [Fact]
    public async Task Enqueue_ShouldAddItemToChannel()
    {
        // Arrange
        var channelService = new ChannelService<string>();
        const string testItem = "test-item";

        // Act
        await channelService.Enqueue(testItem);

        // Assert - Item should be available for dequeue
        await foreach (var item in channelService.Dequeue(CancellationToken.None))
        {
            Assert.Equal(testItem, item);
            break; // Only check first item
        }
    }

    [Fact]
    public async Task EnqueueRange_ShouldAddMultipleItemsToChannel()
    {
        // Arrange
        var channelService = new ChannelService<string>();
        var testItems = new[] { "item1", "item2", "item3" };

        // Act
        await channelService.EnqueueRange(testItems);

        // Assert
        var receivedItems = new List<string>();
        var cancellationTokenSource = new CancellationTokenSource();

        // Cancel after a short delay to prevent infinite waiting
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);
            cancellationTokenSource.Cancel();
        });

        try
        {
            await foreach (var item in channelService.Dequeue(cancellationTokenSource.Token))
            {
                receivedItems.Add(item);
                if (receivedItems.Count == testItems.Length)
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when timeout occurs
        }

        Assert.Equal(testItems.Length, receivedItems.Count);
        Assert.Equal(testItems, receivedItems);
    }

    [Fact]
    public async Task Dequeue_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var channelService = new ChannelService<string>();
        var cancellationTokenSource = new CancellationTokenSource();

        // Act & Assert
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in channelService.Dequeue(cancellationTokenSource.Token))
            {
                // Should not reach here
            }
        });
    }

    [Fact]
    public async Task Complete_ShouldStopDequeueOperation()
    {
        // Arrange
        var channelService = new ChannelService<string>();
        await channelService.Enqueue("test-item");

        // Act
        channelService.Complete();

        // Assert - Dequeue should complete after processing existing items
        var itemsReceived = 0;
        await foreach (var item in channelService.Dequeue(CancellationToken.None))
        {
            itemsReceived++;
        }

        Assert.Equal(1, itemsReceived);
    }

    [Fact]
    public async Task MultipleEnqueues_ShouldMaintainOrder()
    {
        // Arrange
        var channelService = new ChannelService<int>();
        var expectedOrder = Enumerable.Range(1, 10).ToArray();

        // Act
        foreach (var number in expectedOrder)
        {
            await channelService.Enqueue(number);
        }

        // Assert
        var receivedItems = new List<int>();
        var cancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);
            cancellationTokenSource.Cancel();
        });

        try
        {
            await foreach (var item in channelService.Dequeue(cancellationTokenSource.Token))
            {
                receivedItems.Add(item);
                if (receivedItems.Count == expectedOrder.Length)
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when timeout occurs
        }

        Assert.Equal(expectedOrder, receivedItems);
    }

    [Fact]
    public async Task EnqueueWithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        var channelService = new ChannelService<string>();
        var cancellationTokenSource = new CancellationTokenSource();

        // Act & Assert
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await channelService.Enqueue("test", cancellationTokenSource.Token));
    }
}
