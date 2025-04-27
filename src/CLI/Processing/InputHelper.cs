namespace Raven.CLI.Processing;

internal static class InputHelper
{
    internal static string GetRequiredValue(string label, string? providedValue, string? defaultValue = null)
    {
        return GetValueOrPrompt(label, providedValue, defaultValue, false)
               ?? throw new InvalidOperationException("Value is required.");
    }

    internal static string? GetOptionalValue(string label, string? providedValue, string? defaultValue = null)
    {
        return GetValueOrPrompt(label, providedValue, defaultValue, true);
    }

    private static string? GetValueOrPrompt(
        string label,
        string? providedValue,
        string? defaultValue,
        bool allowEmpty)
    {
        if (!string.IsNullOrWhiteSpace(providedValue))
            return providedValue.Trim();

        var suffix = allowEmpty || defaultValue is not null ? " [press Enter to skip]" : "";
        var prompt = defaultValue is not null
            ? $"{label} [{defaultValue}]{suffix}: "
            : $"{label}{suffix}: ";

        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();

            if (defaultValue is not null)
                return defaultValue;

            if (allowEmpty)
                return null;
        }
    }
}