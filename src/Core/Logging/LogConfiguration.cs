namespace Raven.Core.Logging;

public sealed class LogConfiguration
{
    public LogConfiguration(string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath, nameof(outputPath));

        var normalized = Normalize(outputPath);
        EnsureLogsDirectoryExists(normalized);

        OutputDirectoryPath = normalized;
    }

    public string OutputDirectoryPath { get; }

    private static string Normalize(string path) => path.Trim().Replace('\\', '/');

    private static void EnsureLogsDirectoryExists(string path)
    {
        if (!path.EndsWith("logs", StringComparison.OrdinalIgnoreCase))
            path = Path.Combine(path, "logs");

        Directory.CreateDirectory(path);
    }
}