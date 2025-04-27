namespace Raven.CLI.Commands;

internal sealed class ExportSourcesCommand : ICommand
{
    private readonly ILogger _log;
    private readonly IStore _store;

    public ExportSourcesCommand(ILogger log, IStore store)
    {
        _log = log;
        _store = store;
    }

    public string Name => "export";

    public async Task ExecuteAsync(string[] args)
    {
        var outputPath = InputHelper.GetRequiredValue("Output-Path", args.ElementAtOrDefault(0));
        
        var sources = await _store.ListSourcesAsync();
        if (sources.Count == 0)
        {
            _log.Information("No sources found.");
            return;
        }

        var content = JsonConvert.SerializeObject(sources);
        var json = Path.Combine(outputPath, "sources.json");

        try
        {
            await File.WriteAllTextAsync(json, content);
            _log.Information($"Exported {sources.Count} sources to {outputPath}");
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to export sources: {ex.Message}");
        }
    }
}