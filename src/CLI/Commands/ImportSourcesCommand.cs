namespace Raven.CLI.Commands;

internal sealed class ImportSourcesCommand : ICommand
{
    private readonly DataStore _store;
    private readonly ILogger _log;

    public ImportSourcesCommand(DataStore store, ILogger log)
    {
        _store = store;
        _log = log;
    }

    public string Name => "import";

    private const int ExpectedArgsCount = 1;

    public async Task ExecuteAsync(string[] args)
    {
        if (args.Length != ExpectedArgsCount || args.Any(string.IsNullOrWhiteSpace))
        {
            _log.Error("Usage: import <path>");
            return;
        }

        var path = args[0].Trim();
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