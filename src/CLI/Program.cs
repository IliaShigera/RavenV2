var rootConfig = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(prefix: "RAVEN_")
    .Build();

var logConfig = new LogConfiguration(rootConfig["Logging:OutputPath"]!);
var storeConfig = new StoreConfiguration(rootConfig["Store:SqliteConnection"]!);
var telegramConfig = new TelegramConfiguration(
    rootConfig["Telegram:Token"]!,
    rootConfig["Telegram:ChatId"]!
);
var defaultPreviewImage = rootConfig["Telegram:PostSettings:DefaultPreviewImage"]!;

var services = new ServiceCollection();
services.AddSingleton(logConfig);
services.AddSingleton(storeConfig);
services.AddSingleton(telegramConfig);

services.AddSingleton<ILogger>(RootLogger.Create(logConfig));

services.AddSingleton<HttpClient>(_ =>
{
    var http = new HttpClient();
    http.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");

    return http;
});

services.AddSingleton<RssFetcher>();
services.AddSingleton<IDbInitializer, DbInitializer>();
services.AddSingleton<IStore, SqliteStore>();
services.AddSingleton<TelegramClient>();
services.AddSingleton<TelegramPostFactory>(_ => new TelegramPostFactory(defaultPreviewImage));

services.AddScoped<ICommand, AddSourceCommand>();
services.AddScoped<ICommand, ListSourcesCommand>();
services.AddScoped<ICommand, UpdateSourceCommand>();
services.AddScoped<ICommand, RemoveSourceCommand>();
services.AddScoped<ICommand, ImportSourcesCommand>();
services.AddScoped<ICommand, ExportSourcesCommand>();
services.AddScoped<ICommand, RunCommand>();
services.AddScoped<CommandDispatcher>();

await using var sp = services.BuildServiceProvider();
var logger = sp.GetRequiredService<ILogger>();
var dbInitializer = sp.GetRequiredService<IDbInitializer>();
var dispatcher = sp.GetRequiredService<CommandDispatcher>();
try
{
    await dbInitializer.InitializeAsync();

    await dispatcher.DispatchAsync(args);
}
catch (Exception ex)
{
    logger.Error("Unhandled exception", ex);
}