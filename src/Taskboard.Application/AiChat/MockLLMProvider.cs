using Taskboard.Application.Contracts.AiChat;

namespace Taskboard.Application.AiChat;

public sealed class MockLLMProvider : ILLMProvider
{
    public string ModelId => "mock";

    public Task<LLMResponse> CompleteAsync(
        IReadOnlyList<LLMMessage> messages,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var lastUserMessage = messages.LastOrDefault(m => m.Role == "user")?.Content ?? "";
        
        return Task.FromResult(new LLMResponse(
            Content: $"Mock response to: {lastUserMessage}",
            Usage: new LLMUsage(10, 20, 30),
            FinishReason: "stop"));
    }

    public async IAsyncEnumerable<LLMStreamChunk> StreamAsync(
        IReadOnlyList<LLMMessage> messages,
        LLMOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lastUserMessage = messages.LastOrDefault(m => m.Role == "user")?.Content ?? "";
        var response = $"Mock streaming response to: {lastUserMessage}";
        
        foreach (var word in response.Split(' '))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(10, cancellationToken);
            yield return new LLMStreamChunk(ContentDelta: word + " ");
        }
        
        yield return new LLMStreamChunk(
            ContentDelta: null,
            IsComplete: true,
            Usage: new LLMUsage(10, 20, 30));
    }
}