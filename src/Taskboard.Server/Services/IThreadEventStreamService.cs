namespace Taskboard.Server.Services;

public interface IThreadEventStreamService
{
    IAsyncEnumerable<ServerSentEvent> SubscribeAsync(string threadId, CancellationToken cancellationToken = default);

    Task PublishAsync(string threadId, ServerSentEvent serverSentEvent, CancellationToken cancellationToken = default);
}
