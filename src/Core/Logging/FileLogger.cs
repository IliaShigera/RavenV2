namespace Raven.Core.Logging;

public sealed class FileLogger : ILogger
{
    private readonly string _outputDirectoryPath;

    public FileLogger(string outputDirectoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectoryPath, nameof(outputDirectoryPath));
        _outputDirectoryPath = outputDirectoryPath;
    }

    public void Log(string message, LogLevel lv)
    {
        if (lv != LogLevel.Error) 
            return;
        
        var log = $"{DateTime.UtcNow:O} [{lv}] {message}";
        var path = Path.Combine(_outputDirectoryPath, $"{DateTime.Now:yyyy-MM}.txt");

    
        File.AppendAllText(path, log, Encoding.UTF8);
    }
}