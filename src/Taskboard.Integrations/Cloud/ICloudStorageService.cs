namespace Taskboard.Integrations.Cloud;

public interface ICloudStorageService
{
    Task<string> UploadAsync(Stream content, string key, CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
