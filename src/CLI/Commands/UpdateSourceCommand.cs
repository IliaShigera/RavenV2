namespace Raven.CLI.Commands;

internal sealed class UpdateSourceCommand : ICommand
{
    private readonly ILogger _log;
    private readonly IStore _store;

    public UpdateSourceCommand(ILogger log, IStore store)
    {
        _log = log;
        _store = store;
    }

    public string Name => "update";

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
            _log.Error("Source with this id does not exist.");
            return;
        }

        var name = InputHelper.GetRequiredValue("Name", args.ElementAtOrDefault(1), source.Name);
        var url = InputHelper.GetRequiredValue("URL", args.ElementAtOrDefault(2), source.Url);
        var feed = InputHelper.GetRequiredValue("Feed", args.ElementAtOrDefault(3), source.Feed);
        var image = InputHelper.GetOptionalValue("Image", args.ElementAtOrDefault(4), source.Image);

        source.Update(name, url, feed, image);
        await _store.UpdateSourceAsync(source);
    }

    private static string Prompt(string label, string current)
    {
        Console.Write($"{label} [{current}]: ");
        var input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? current : input.Trim();
    }
}