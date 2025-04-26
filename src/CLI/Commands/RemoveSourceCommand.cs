namespace Raven.CLI.Commands;

internal sealed class RemoveSourceCommand : ICommand
{
    private readonly DataStore _store;
    private readonly ILogger _log;

    public RemoveSourceCommand(DataStore store, ILogger log)
    {
        _store = store;
        _log = log;
    }

    public string Name => "rm";

    private const int ExpectedArgsCount = 1;

    public async Task ExecuteAsync(string[] args)
    {
        if (args.Length != ExpectedArgsCount || args.Any(string.IsNullOrWhiteSpace))
        {
            _log.Error("Usage: rm <id>");
            return;
        }

        if (!int.TryParse(args[0], out var id))
        {
            _log.Error($"Invalid ID: {args[0]}");
            return;
        }

        var source = await _store.FindSourceByIdAsync(id);
        if (source is null)
        {
            _log.Information("Source with this id doest exist.");
            return;
        }

        await _store.RemoveSourceAsync(source);

        _log.Information($"Removed source: {source.Name} [{source.Url}] with feed [{source.Feed}]");
    }
}