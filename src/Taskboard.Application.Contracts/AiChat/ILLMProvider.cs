namespace Taskboard.Application.Contracts.AiChat;

public interface ILLMProvider
{
    string ModelId { get; }
    
    Task<LLMResponse> CompleteAsync(
        IReadOnlyList<LLMMessage> messages,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<LLMStreamChunk> StreamAsync(
        IReadOnlyList<LLMMessage> messages,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default);
}

public sealed record LLMMessage(
    string Role,
    string Content,
    string? Name = null);

public sealed record LLMOptions(
    double? Temperature = null,
    int? MaxTokens = null,
    double? TopP = null,
    IReadOnlyList<string>? StopSequences = null);

public sealed record LLMResponse(
    string Content,
    LLMUsage? Usage = null,
    string? FinishReason = null);

public sealed record LLMUsage(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens);

public sealed record LLMStreamChunk(
    string? ContentDelta = null,
    bool IsComplete = false,
    LLMUsage? Usage = null);