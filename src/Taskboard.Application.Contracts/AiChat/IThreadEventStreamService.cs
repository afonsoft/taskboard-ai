namespace Taskboard.Application.Contracts.AiChat;

public interface IThreadEventStreamService
{
    IAsyncEnumerable<ServerSentEvent> SubscribeAsync(string threadId, CancellationToken cancellationToken = default);

    Task PublishAsync(string threadId, ServerSentEvent serverSentEvent, CancellationToken cancellationToken = default);
}

public sealed record ServerSentEvent(string Type, object Payload);