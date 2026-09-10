using System;
using Shouldly;
using Taskboard.Application.Contracts.Configuration;
using Xunit;

namespace Taskboard.Tests.Unit.Configuration;

public class TaskboardEnvironmentTests
{
    [Fact]
    public void GetPort_WhenTaskboardPortIsSet_ShouldReturnConfiguredValue()
    {
        // Covers RF-001: Rename server port variable
        using var _ = SetEnv("TASKBOARD_PORT", "8080");

        var port = TaskboardEnvironment.GetPort();

        port.ShouldBe("8080");
    }

    [Fact]
    public void GetPort_WhenTaskboardPortIsNotSet_ShouldReturnDefault()
    {
        // Covers RF-001: Rename server port variable
        using var _ = ClearEnv("TASKBOARD_PORT");

        var port = TaskboardEnvironment.GetPort();

        port.ShouldBe("47823");
    }

    [Fact]
    public void GetPort_WhenOnlyLegacyCodexPortIsSet_ShouldIgnoreAndReturnDefault()
    {
        // Covers breaking change: no fallback to CODEX_* variables
        using var _1 = ClearEnv("TASKBOARD_PORT");
        using var _2 = SetEnv("CODEX_TASKBOARD_PORT", "9000");

        var port = TaskboardEnvironment.GetPort();

        port.ShouldBe("47823");
    }

    [Fact]
    public void GetDataDir_WhenTaskboardDataDirIsSet_ShouldReturnConfiguredValue()
    {
        // Covers RF-002: Rename data directory variable
        using var _ = SetEnv("TASKBOARD_DATA_DIR", "/var/taskboard");

        var dataDir = TaskboardEnvironment.GetDataDir("/fallback");

        dataDir.ShouldBe("/var/taskboard");
    }

    [Fact]
    public void GetDataDir_WhenTaskboardDataDirIsNotSet_ShouldReturnDefaultUnderBasePath()
    {
        // Covers RF-002: Rename data directory variable
        using var _ = ClearEnv("TASKBOARD_DATA_DIR");

        var dataDir = TaskboardEnvironment.GetDataDir("/app");

        dataDir.ShouldBe("/app/.data");
    }

    [Fact]
    public void GetDataDir_WhenOnlyLegacyCodexDataDirIsSet_ShouldIgnoreAndReturnDefault()
    {
        // Covers breaking change: no fallback to CODEX_* variables
        using var _1 = ClearEnv("TASKBOARD_DATA_DIR");
        using var _2 = SetEnv("CODEX_TASKBOARD_DATA_DIR", "/old");

        var dataDir = TaskboardEnvironment.GetDataDir("/app");

        dataDir.ShouldBe("/app/.data");
    }

    [Fact]
    public void GetServerUrls_WhenTaskboardPortIsSet_ShouldReturnLocalhostUrl()
    {
        // Covers RF-001: server URLs derive from TASKBOARD_PORT
        using var _ = SetEnv("TASKBOARD_PORT", "8080");

        var urls = TaskboardEnvironment.GetServerUrls();

        urls.ShouldBe("http://127.0.0.1:8080");
    }

    [Fact]
    public void GetServerUrls_WhenAspNetCoreUrlsIsSet_ShouldReturnAspNetCoreUrls()
    {
        // Covers existing ASPNETCORE_URLS override behavior
        using var _ = SetEnv("ASPNETCORE_URLS", "http://0.0.0.0:5000");

        var urls = TaskboardEnvironment.GetServerUrls();

        urls.ShouldBe("http://0.0.0.0:5000");
    }

    private static IDisposable SetEnv(string name, string value)
    {
        var original = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, value);
        return new EnvRestorer(name, original);
    }

    private static IDisposable ClearEnv(string name)
    {
        var original = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, null);
        return new EnvRestorer(name, original);
    }

    private sealed class EnvRestorer : IDisposable
    {
        private readonly string _name;
        private readonly string? _value;

        public EnvRestorer(string name, string? value)
        {
            _name = name;
            _value = value;
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(_name, _value);
        }
    }
}
