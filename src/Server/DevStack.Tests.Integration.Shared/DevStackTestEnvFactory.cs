namespace DevStack.Tests.Integration.Shared;

public static class DevStackTestEnvFactory
{
    public static DevStackTestEnvBuilder CreateBuilder()
    {
        return new DevStackTestEnvBuilder();
    }

    public static DevStackTestEnv CreateApi(string? solutionDir = null)
    {
        return CreateBuilder()
            .WithSolutionDir(solutionDir ?? FindSolutionDirectory())
            .WithMode(DevStackTestEnvMode.Api)
            .Build();
    }

    public static DevStackTestEnv CreateMcp(string? solutionDir = null)
    {
        return CreateBuilder()
            .WithSolutionDir(solutionDir ?? FindSolutionDirectory())
            .WithMode(DevStackTestEnvMode.Mcp)
            .Build();
    }

    public static string FindSolutionDirectory()
    {
        string? currentDir = AppDomain.CurrentDomain.BaseDirectory;

        while (!string.IsNullOrEmpty(currentDir))
        {
            if (Directory.GetFiles(currentDir, "DevStack.slnx").Any() ||
                Directory.GetFiles(currentDir, "*.sln").Any())
            {
                return currentDir;
            }

            string? parentDir = Path.GetDirectoryName(currentDir);
            if (parentDir is null || parentDir == currentDir)
            {
                break;
            }

            currentDir = parentDir;
        }

        string testProjectDir = Path.GetDirectoryName(typeof(DevStackTestEnvFactory).Assembly.Location)
            ?? throw new InvalidOperationException("Could not determine assembly directory.");

        return Path.GetFullPath(Path.Combine(testProjectDir, "..", "..", "..", ".."));
    }
}
