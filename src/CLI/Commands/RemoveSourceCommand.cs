namespace Raven.CLI.Commands;

internal sealed class RemoveSourceCommand : ICommand
{
    private readonly IStore _store;
    private readonly ILogger _log;

    public RemoveSourceCommand(IStore store, ILogger log)
    {
        _store = store;
        _log = log;
    }

    public string Name => "rm";

    public async Task ExecuteAsync(string[] args)
    {
        var idString = InputHelper.GetRequiredValue("ID", args.ElementAtOrDefault(0));
        if (!int.TryParse(idString, out var id))
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