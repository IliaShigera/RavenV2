using Raven.Core.Abstractions;

namespace Raven.Core.Logging.Extensions;

public static class LoggerExtensions
{
    public static void Information(this ILogger l, string message)
    {
        l.Log(message, LogLevel.Info);
    }

    public static void Warning(this ILogger l, string message)
    {
        l.Log(message, LogLevel.Warning);
    }

    public static void Error(this ILogger l, string message, Exception? ex = null)
    {
        var fullMessage = ex is not null
            ? $"{message}\n{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}"
            : message;

        l.Log(fullMessage, LogLevel.Error);
    }
}