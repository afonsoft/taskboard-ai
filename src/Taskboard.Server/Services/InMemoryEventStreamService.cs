using System.Threading.Channels;

namespace Taskboard.Server.Services;

public sealed class InMemoryEventStreamService : IEventStreamService, IDisposable
{
    private readonly Channel<ServerSentEvent> _channel = Channel.CreateUnbounded<ServerSentEvent>();

    public IAsyncEnumerable<ServerSentEvent> SubscribeAsync(CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAllAsync(cancellationToken);

    public Task PublishAsync(ServerSentEvent serverSentEvent, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(serverSentEvent, cancellationToken).AsTask();

    public void Dispose()
    {
        _channel.Writer.Complete();
    }
}
