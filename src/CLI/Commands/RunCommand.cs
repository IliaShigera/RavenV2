namespace Raven.CLI.Commands;

internal sealed class RunCommand : ICommand
{
    private readonly ILogger _log;
    private readonly TelegramClient _client;
    private readonly TelegramPostFactory _tgPostFactory;
    private readonly RssFetcher _fetcher;
    private readonly IStore _store;

    public RunCommand(
        ILogger log,
        TelegramClient client, 
        TelegramPostFactory tgPostFactory,
        RssFetcher fetcher, 
        IStore store)
    {
        _log = log;
        _client = client;
        _tgPostFactory = tgPostFactory;
        _fetcher = fetcher;
        _store = store;
    }


    public string Name => "run";

    public async Task ExecuteAsync(string[] args)
    {
        var sources = await _store.ListSourcesAsync();

        if (!sources.Any())
        {
            _log.Information("No sources found. Exiting.");
            return;
        }

        foreach (var source in sources)
            await ProcessSourceAsync(source);
    }

    private async Task ProcessSourceAsync(Source source)
    {
        _log.Information($"\nFetching feed for: {source.Name} [{source.Feed}]");

        var result = await _fetcher.FetchAsync(source);
        if (!result.Ok)
        {
            _log.Warning($"Failed to fetch: {result.Error}");
            return;
        }

        var fetchedRecent = result.Data.Where(p => p.PublishedAt > source.LastFetchedAt);
        var unsent = await _store.ListUnsentPostsAsync(source.Id);
        var posts = fetchedRecent.Concat(unsent).DistinctBy(p => p.Link).ToList();

        if (!posts.Any())
        {
            _log.Information("No new posts.");
            await UpdateSourceLastFetchAsync(source);
            return;
        }

        await _store.AddPostsIfNotExistsAsync(posts);
        await UpdateSourceLastFetchAsync(source);

        _log.Information($"Sending {posts.Count} posts to Telegram...");

        var sentCount = await SendPostsAsync(source, posts);
        await _store.UpdatePostsAsync(posts);

        _log.Information($"Sent {sentCount}/{posts.Count} posts");
        _log.Information(new string('=', 60));
    }

    private async Task<int> SendPostsAsync(Source source, List<Post> posts)
    {
        var sent = 0;
        foreach (var post in posts)
        {
            var telegramPost = _tgPostFactory.CreateFrom(source, post);
            var response = await _client.SendPostAsync(telegramPost);
            if (!response.Ok)
            {
                _log.Warning($"Failed to send: {response.Error}");
                continue;
            }

            _log.Information($"Sent: {post.Title}");
            post.MarkAsSent();
            sent++;
        }

        return sent;
    }

    private async Task UpdateSourceLastFetchAsync(Source source)
    {
        source.UpdateLastFetchedAt();
        await _store.UpdateSourceAsync(source);
    }
}