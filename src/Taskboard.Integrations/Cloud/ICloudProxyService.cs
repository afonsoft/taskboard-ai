namespace Taskboard.Integrations.Cloud;

public interface ICloudProxyService
{
    Task<Stream> GetAsync(string path, CancellationToken cancellationToken = default);

    Task<string> PostAsync(string path, object payload, CancellationToken cancellationToken = default);
}
