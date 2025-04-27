namespace Raven.Core.Abstractions;

public interface ILogger
{
    void Log(string message, LogLevel level);
}