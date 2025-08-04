// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System.Collections.Generic;
using System.Threading;

namespace Dalog.Foundation.BackgroundServices.Channel;

internal interface IChannelReader<out TQueueItem>
{
    IAsyncEnumerable<TQueueItem> Dequeue(CancellationToken cancellationToken = default);

    void Complete();
}
