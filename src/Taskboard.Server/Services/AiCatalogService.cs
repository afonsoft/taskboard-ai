using System.Collections.Concurrent;
using Taskboard.Dtos;

namespace Taskboard.Server.Services;

public sealed class AiCatalogService
{
    private readonly ConcurrentDictionary<string, AiChatModelDto> _models = new();

    public AiCatalogService()
    {
        AddDefaultModels();
    }

    public IReadOnlyCollection<AiChatModelDto> List() => _models.Values.ToList().AsReadOnly();

    public bool TryAdd(AiChatModelDto model) => _models.TryAdd(model.Id, model);

    public bool Contains(string modelId) => _models.ContainsKey(modelId);

    private void AddDefaultModels()
    {
        TryAdd(new AiChatModelDto("gpt-4o", "openai", "GPT-4o", true));
        TryAdd(new AiChatModelDto("gpt-4o-mini", "openai", "GPT-4o mini", false));
        TryAdd(new AiChatModelDto("claude-sonnet-4", "anthropic", "Claude Sonnet 4", true));
        TryAdd(new AiChatModelDto("claude-haiku-4", "anthropic", "Claude Haiku 4", false));
    }
}
