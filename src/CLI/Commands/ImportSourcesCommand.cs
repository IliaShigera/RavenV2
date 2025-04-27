namespace Raven.CLI.Commands;

internal sealed class ImportSourcesCommand : ICommand
{
    private readonly IStore _store;
    private readonly ILogger _log;

    public ImportSourcesCommand(IStore store, ILogger log)
    {
        _store = store;
        _log = log;
    }

    public string Name => "import";

    public async Task ExecuteAsync(string[] args)
    {
        var path = InputHelper.GetRequiredValue("Path", args.ElementAtOrDefault(0));
        if (!File.Exists(path))
        {
            _log.Error("File not found.");
            return;
        }

        var json = await File.ReadAllTextAsync(path);
        var parsed = JsonConvert.DeserializeObject<List<Source>>(json) ?? [];

        if (!parsed.Any())
        {
            _log.Information("No sources found in the file.");
            return;
        }

        List<Source> newSources = [];
        foreach (var source in parsed)
        {
            if (!await _store.IsFeedUniqueAsync(source.Feed))
            {
                _log.Warning($"Skipping source {source.Name} with duplicate feed [{source.Feed}]");
                continue;
            }

            newSources.Add(source);
        }

        await _store.AddSourcesAsync(newSources);
        _log.Information($"Added {newSources.Count} new sources");
    }
}