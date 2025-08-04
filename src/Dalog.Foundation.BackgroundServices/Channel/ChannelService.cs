// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Dalog.Foundation.BackgroundServices.Channel;

internal sealed class ChannelService<TQueueItem> : IChannelWriter<TQueueItem>, IChannelReader<TQueueItem>
{
    private readonly Channel<TQueueItem> _channel = System.Threading.Channels.Channel.CreateUnbounded<TQueueItem>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
        AllowSynchronousContinuations = false
    });

    public ValueTask Enqueue(TQueueItem item, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(item, cancellationToken);

    public async ValueTask EnqueueRange(IEnumerable<TQueueItem> item, CancellationToken cancellationToken = default)
    {
        foreach (var image in item)
        {
            await Enqueue(image, cancellationToken);
        }
    }

    public async IAsyncEnumerable<TQueueItem> Dequeue([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return item;
        }
    }

    public void Complete() => _channel.Writer.Complete();
}
