using Raven.Core.Abstractions;

namespace Raven.Core.Logging.Sinks;

public sealed class ConsoleLogger : ILogger
{
    public void Log(string message, LogLevel level)
    {
        Console.WriteLine(message);
    }
}