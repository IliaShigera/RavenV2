namespace Raven.Core.Sqlite;

public sealed class DbInitializer
{
    private readonly StoreConfiguration _config;

    public DbInitializer(StoreConfiguration config)
    {
        _config = config;
    }


    public async Task InitializeAsync()
    {
        await using var conn = new SqliteConnection(_config.ConnectionString);
        await conn.OpenAsync();

        await using var command = conn.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Sources (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Url TEXT NOT NULL,
                Feed TEXT NOT NULL UNIQUE,
                Image TEXT,
                LastFetchedAt TEXT
            );

            CREATE TABLE IF NOT EXISTS Posts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Desc TEXT,
                Thumbnail TEXT,
                Link TEXT NOT NULL,
                PublishedAt TEXT,
                SentAt TEXT,
                SourceId INTEGER NOT NULL,
                FOREIGN KEY (SourceId) REFERENCES Sources(Id) ON DELETE CASCADE,
                UNIQUE (Link, SourceId)
            );
            """;

        await command.ExecuteNonQueryAsync();
    }
}