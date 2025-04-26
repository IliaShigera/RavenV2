namespace Raven.CLI.Commands;

internal sealed class ListSourcesCommand : ICommand
{
    private readonly DataStore _store;
    private readonly ILogger _log;

    public ListSourcesCommand(DataStore store, ILogger log)
    {
        _store = store;
        _log = log;
    }

    public string Name => "ls";

    public async Task ExecuteAsync(string[] args)
    {
        if (args.Length != 0)
        {
            _log.Error("Usage: ls");
            return;
        }

        var sources = await _store.ListSourcesAsync();
        if (sources.Count == 0)
        {
            _log.Information("No sources found.");
            return;
        }

        foreach (var s in sources)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{"Name",-15} {s.Name}");
            sb.AppendLine($"{"Id",-15} {s.Id}");
            sb.AppendLine($"{"Url",-15} {s.Url}");
            sb.AppendLine($"{"Feed",-15} {s.Feed}");
            sb.AppendLine($"{"Image",-15} {s.Image ?? "not set"}");
            sb.AppendLine(
                $"{"Last Fetch",-15} {s.LastFetchedAt?.ToString("yyyy-MM-dd HH:mm") ?? "not processed yet"}");
            sb.AppendLine(new string('-', 70));

            Console.Write(sb.ToString());
        }
    }
}