namespace Raven.CLI.Commands;

internal sealed class AddSourceCommand : ICommand
{
    private readonly DataStore _store;
    private readonly ILogger _log;

    public AddSourceCommand(DataStore store, ILogger log)
    {
        _store = store;
        _log = log;
    }

    public string Name => "add";
    
    private const int ExpectedArgsCount = 4;

    public async Task ExecuteAsync(string[] args)
    {
        if (args.Length != ExpectedArgsCount || args.Any(string.IsNullOrWhiteSpace))
        {
            _log.Error("Usage: add-source <name> <url> <feed> <image>");
            return;
        }

        ParseArgs(args, out var name, out var url, out var feed, out var image);

        if (!await _store.IsFeedUniqueAsync(feed))
        {
            _log.Error($"Source with the same feed [{feed}] already exists.");
            return;
        }

        var source = Source.Create(name, url, feed, image);
        await _store.AddSourcesAsync([source]);

        _log.Information($"Added source: {name} [{url}] with feed [{feed}] and image [{image}]");
    }

    private static void ParseArgs(string[] args, out string name, out string url, out string feed, out string image)
    {
        name = args[0].Trim();
        url = args[1].Trim();
        feed = args[2].Trim();
        image = args[3].Trim();
    }
}