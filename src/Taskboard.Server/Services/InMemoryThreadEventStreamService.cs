using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Taskboard.Server.Services;

public sealed class InMemoryThreadEventStreamService : IThreadEventStreamService, IDisposable
{
    private readonly ConcurrentDictionary<string, Channel<ServerSentEvent>> _channels = new();

    public IAsyncEnumerable<ServerSentEvent> SubscribeAsync(string threadId, CancellationToken cancellationToken = default)
    {
        var channel = _channels.GetOrAdd(threadId, _ => Channel.CreateUnbounded<ServerSentEvent>());
        return channel.Reader.ReadAllAsync(cancellationToken);
    }

    public Task PublishAsync(string threadId, ServerSentEvent serverSentEvent, CancellationToken cancellationToken = default)
    {
        var channel = _channels.GetOrAdd(threadId, _ => Channel.CreateUnbounded<ServerSentEvent>());
        return channel.Writer.WriteAsync(serverSentEvent, cancellationToken).AsTask();
    }

    public void Dispose()
    {
        foreach (var channel in _channels.Values)
        {
            channel.Writer.Complete();
        }

        _channels.Clear();
    }
}
