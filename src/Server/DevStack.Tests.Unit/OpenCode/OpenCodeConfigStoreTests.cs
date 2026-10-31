using AwesomeAssertions;

using DevStack.OpenCode.Models;
using DevStack.OpenCode.Options;
using DevStack.OpenCode.Store;

using Microsoft.Extensions.Options;

using Xunit;

namespace DevStack.Tests.Unit.OpenCode;

public class OpenCodeConfigStoreTests : IDisposable
{
    private readonly string _tempDir;

    public OpenCodeConfigStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "devstack-opencode-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        GC.SuppressFinalize(this);
    }

    private OpenCodeConfigStore CreateStore(string? defaultPath = null)
    {
        var options = Options.Create(new OpenCodeOptions
        {
            DefaultConfigPath = defaultPath,
        });
        return new OpenCodeConfigStore(options);
    }

    [Fact]
    public async Task LoadAsync_FileDoesNotExist_ReturnsEmptyConfig()
    {
        var store = CreateStore();
        var path = Path.Combine(_tempDir, "missing.json");

        var config = await store.LoadAsync(path, TestContext.Current.CancellationToken);

        config.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadAsync_EmptyPath_Throws()
    {
        var store = CreateStore();

        var act = () => store.LoadAsync("");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SaveAsync_WritesValidJson()
    {
        var store = CreateStore();
        var path = Path.Combine(_tempDir, "opencode.json");
        var config = new OpenCodeConfig
        {
            Model = "anthropic/claude-3-5-sonnet",
            LogLevel = LogLevel.Debug,
        };

        await store.SaveAsync(path, config, TestContext.Current.CancellationToken);

        File.Exists(path).Should().BeTrue();
        var json = await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);
        json.Should().Contain("\"model\": \"anthropic/claude-3-5-sonnet\"");
        json.Should().Contain("\"logLevel\": \"DEBUG\"");
    }

    [Fact]
    public async Task SaveAsync_CreatesMissingDirectory()
    {
        var store = CreateStore();
        var nested = Path.Combine(_tempDir, "nested", "deep", "opencode.json");
        var config = new OpenCodeConfig { Model = "anthropic/claude" };

        await store.SaveAsync(nested, config, TestContext.Current.CancellationToken);

        File.Exists(nested).Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_RoundTripsSavedConfig()
    {
        var store = CreateStore();
        var path = Path.Combine(_tempDir, "opencode.json");
        var original = new OpenCodeConfig
        {
            Model = "anthropic/claude-3-5-sonnet",
            LogLevel = LogLevel.Info,
            Permission = PermissionConfig.FromAction(PermissionAction.Deny),
        };

        await store.SaveAsync(path, original, TestContext.Current.CancellationToken);
        var loaded = await store.LoadAsync(path, TestContext.Current.CancellationToken);

        loaded.Model.Should().Be(original.Model);
        loaded.LogLevel.Should().Be(original.LogLevel);
        loaded.Permission!.Action.Should().Be(PermissionAction.Deny);
    }

    [Fact]
    public async Task LoadDefaultAsync_NoExplicitPath_NoFileFound_ReturnsEmptyConfig()
    {
        // Use a temp dir as the working directory so that ./opencode.json is not present
        // and we cannot leak into the user's home directory.
        var currentDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_tempDir);
        try
        {
            var store = CreateStore();
            var result = await store.LoadDefaultAsync(TestContext.Current.CancellationToken);

            result.Config.Should().NotBeNull();
            result.Path.Should().EndWith("opencode.json");
        }
        finally
        {
            Directory.SetCurrentDirectory(currentDir);
        }
    }

    [Fact]
    public async Task LoadDefaultAsync_ExplicitPath_LoadsFromExplicitPath()
    {
        var explicitPath = Path.Combine(_tempDir, "custom-opencode.json");
        await File.WriteAllTextAsync(
            explicitPath,
            """{ "model": "explicit-model" }""",
            TestContext.Current.CancellationToken);
        var store = CreateStore(explicitPath);

        var result = await store.LoadDefaultAsync(TestContext.Current.CancellationToken);

        result.Path.Should().Be(explicitPath);
        result.Config.Model.Should().Be("explicit-model");
    }

    [Fact]
    public async Task SaveAsync_EmptyPath_Throws()
    {
        var store = CreateStore();

        var act = () => store.SaveAsync("", new OpenCodeConfig());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SaveAsync_NullConfig_Throws()
    {
        var store = CreateStore();

        var act = () => store.SaveAsync("anywhere.json", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
