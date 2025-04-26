namespace Raven.Core.Logging;

public sealed class ConsoleLogger : ILogger
{
    public void Log(string message, LogLevel level)
    {
        Console.WriteLine(message);
    }
}