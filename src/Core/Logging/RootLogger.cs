namespace Raven.Core.Logging;

public sealed class RootLogger : ILogger
{
    private readonly ILogger _inner;

    public RootLogger(ILogger inner) => _inner = inner;

    public void Log(string message, LogLevel level) => _inner.Log(message, level);
    
    public static RootLogger Create(LogConfiguration config)
    {
        var consoleLogger = new ConsoleLogger();
        var fileLogger = new FileLogger(config.OutputDirectoryPath);
        var multiLogger = new MultiLogger([consoleLogger, fileLogger]);
        return new RootLogger(multiLogger);
    }
}