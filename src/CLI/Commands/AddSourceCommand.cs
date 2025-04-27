namespace Raven.CLI.Commands;

internal sealed class AddSourceCommand : ICommand
{
    private readonly IStore _store;
    private readonly ILogger _log;

    public AddSourceCommand(IStore store, ILogger log)
    {
        _store = store;
        _log = log;
    }

    public string Name => "add";

    public async Task ExecuteAsync(string[] args)
    {
        var name = InputHelper.GetRequiredValue("Name", args.ElementAtOrDefault(0));
        var url = InputHelper.GetRequiredValue("URL", args.ElementAtOrDefault(1));
        var feed = InputHelper.GetRequiredValue("Feed", args.ElementAtOrDefault(0));
        var image = InputHelper.GetOptionalValue("Image", args.ElementAtOrDefault(0));

        if (!await _store.IsFeedUniqueAsync(feed))
        {
            _log.Error($"Source with the same feed [{feed}] already exists.");
            return;
        }

        var source = Source.Create(name, url, feed, image);
        await _store.AddSourcesAsync([source]);

        _log.Information($"Added source: {name} [{url}] with feed [{feed}] and image [{image}]");
    }
}