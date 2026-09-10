using System;

namespace Taskboard.Application.Contracts.Configuration;

/// <summary>
/// Resolves taskboard environment variables.
/// </summary>
/// <remarks>
/// This class intentionally does not fall back to legacy <c>CODEX_TASKBOARD_*</c>
/// variables. Use the new <c>TASKBOARD_*</c> names only.
/// </remarks>
public static class TaskboardEnvironment
{
    private const string DefaultPort = "47823";
    private const string DefaultDataDirName = ".data";

    /// <summary>
    /// Returns the configured taskboard port. Defaults to <c>47823</c>.
    /// </summary>
    public static string GetPort()
    {
        return GetTrimmedOrDefault("TASKBOARD_PORT", DefaultPort);
    }

    /// <summary>
    /// Returns the configured taskboard data directory. Defaults to
    /// <c>{basePath}/.data</c>.
    /// </summary>
    public static string GetDataDir(string basePath)
    {
        var configured = GetTrimmedOrDefault("TASKBOARD_DATA_DIR", string.Empty);
        if (string.IsNullOrEmpty(configured))
        {
            return System.IO.Path.Combine(basePath, DefaultDataDirName);
        }

        return configured;
    }

    /// <summary>
    /// Returns the server URLs. <c>ASPNETCORE_URLS</c> takes precedence;
    /// otherwise falls back to <c>http://127.0.0.1:{TASKBOARD_PORT}</c>.
    /// </summary>
    public static string GetServerUrls()
    {
        var aspNetCoreUrls = GetTrimmedOrDefault("ASPNETCORE_URLS", string.Empty);
        if (!string.IsNullOrEmpty(aspNetCoreUrls))
        {
            return aspNetCoreUrls;
        }

        return $"http://127.0.0.1:{GetPort()}";
    }

    private static string GetTrimmedOrDefault(string name, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
    }
}
