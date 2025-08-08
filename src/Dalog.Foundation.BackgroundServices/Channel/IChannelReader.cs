// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

using System.Collections.Generic;
using System.Threading;

namespace Dalog.Foundation.BackgroundServices.Channel;

internal interface IChannelReader<out TMessage>
{
    IAsyncEnumerable<TMessage> Dequeue(CancellationToken cancellationToken = default);

    void Complete();
}
