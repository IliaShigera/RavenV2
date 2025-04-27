namespace Raven.CLI.Processing;

internal sealed class CommandDispatcher
{
    private readonly ILogger _log;
    private Dictionary<string, ICommand> _registry;

    public CommandDispatcher(IEnumerable<ICommand> commands, ILogger log)
    {
        _log = log;
        _registry = commands.ToDictionary(c => c.Name);
    }

    internal async Task DispatchAsync(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help" or "help" or "-h" or "?")
        {
            PrintHelp();
            return;
        }

        if (!_registry.TryGetValue(args[0], out var command))
        {
            _log.Error($"Unknown command: {args[0]}");
            PrintHelp();
            return;
        }

        await command.ExecuteAsync(args.Skip(1).ToArray());
    }


    private static void PrintHelp()
    {
        const string message =
            """
            Raven - A simple content scraping pipeline
            Fetches new content from RSS feeds sending it to Telegram.
            Usage: raven <command> [args]
                    
            Commands:
                run                  Fetches new posts from all configured sources and sends them to Telegram.
                import <path>        Imports new sources from JSON file. (see sources.example.json)
                export <path>        Exports all sources to JSON file.
                                     Example: raven export sources.json
                add <name> <url> <feed-url> <image-url>   Add a new source.
                                     Example: raven add "Tech Blog" "https://techblog.com "https://techblog.com/rss" "https://techblog.com/image.png"
                ls                   List all configured sources.
                Update <id> <name> <url> <feed-url> <image-url>   Update a source by ID.
                rm <id>              Remove a source by ID.
                                     Example: raven remove 1
                help                 Show this message.
                    
            Configuration:
                Edit appsettings.json to set API keys (Telegram), 
                see appsettings.example.json.
            """;

        Console.WriteLine(message);
    }
}