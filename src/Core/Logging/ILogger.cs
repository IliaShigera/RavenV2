namespace Raven.Core.Logging;

public interface ILogger
{
    void Log(string message, LogLevel level);
}