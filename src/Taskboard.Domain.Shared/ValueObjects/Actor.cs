namespace Taskboard.ValueObjects;

public sealed record Actor
{
    private static readonly IReadOnlyCollection<string> AllowedTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "user",
        "agent"
    };

    public string Type { get; }
    public string Id { get; }
    public string Name { get; }
    public string? AvatarUrl { get; }

    public Actor(string type, string id, string name, string? avatarUrl = null)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new DomainException(TaskboardDomainErrorCodes.InvalidActorType, "Actor type cannot be empty.");
        }

        if (!AllowedTypes.Contains(type))
        {
            throw new DomainException(TaskboardDomainErrorCodes.InvalidActorType, $"'{type}' is not a valid actor type.");
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Actor id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Actor name cannot be empty.", nameof(name));
        }

        Type = type;
        Id = id;
        Name = name;
        AvatarUrl = avatarUrl;
    }

    public static Actor User(string id, string name, string? avatarUrl = null) => new("user", id, name, avatarUrl);

    public static Actor Agent(string id, string name, string? avatarUrl = null) => new("agent", id, name, avatarUrl);

    public static Actor LocalUser() => new("user", "local", "Local User");
}
