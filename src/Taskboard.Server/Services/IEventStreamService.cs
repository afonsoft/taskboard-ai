namespace Taskboard.Server.Services;

public interface IEventStreamService
{
    IAsyncEnumerable<ServerSentEvent> SubscribeAsync(CancellationToken cancellationToken = default);

    Task PublishAsync(ServerSentEvent serverSentEvent, CancellationToken cancellationToken = default);
}
