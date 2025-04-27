using Raven.Core.Abstractions;

namespace Raven.Core.Logging;

public sealed class MultiLogger : ILogger
{
    private readonly IEnumerable<ILogger> _loggers;

    public MultiLogger(IEnumerable<ILogger> loggers)
    {
        _loggers = loggers;
    }

    public void Log(string message, LogLevel level)
    {
        foreach (var logger in _loggers)
        {
            logger.Log(message, level);
        }
    }
}