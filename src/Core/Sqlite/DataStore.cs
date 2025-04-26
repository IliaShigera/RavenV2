namespace Raven.Core.Sqlite;

public sealed class DataStore
{
    private readonly StoreConfiguration _config;

    public DataStore(StoreConfiguration config)
    {
        _config = config;
    }

    public async Task AddSourcesAsync(IEnumerable<Source> sources)
    {
        await using var conn = new SqliteConnection(_config.ConnectionString);
        await conn.OpenAsync();

        await using var command = conn.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Sources (Name, Url, Feed, Image)
            VALUES (@name, @url, @feed, @image);
            """;

        foreach (var s in sources)
        {
            command.Parameters.Clear();

            command.Parameters.AddWithValue("@name", s.Name);
            command.Parameters.AddWithValue("@url", s.Url);
            command.Parameters.AddWithValue("@feed", s.Feed);
            command.Parameters.AddWithValue("@image", (object?)s.Image ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<IReadOnlyList<Source>> ListSourcesAsync()
    {
        await using var conn = new SqliteConnection(_config.ConnectionString);
        await conn.OpenAsync();

        await using var command = conn.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Name, Url, Feed, Image, LastFetchedAt
            FROM Sources;
            """;

        var sources = new List<Source>();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            sources.Add(Source.FromDb(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            ));
        }

        return sources.AsReadOnly();
    }

    public async Task<Source?> FindSourceByIdAsync(int id)
    {
        await using var conn = new SqliteConnection(_config.ConnectionString);
        await conn.OpenAsync();

        await using var command = conn.CreateCommand();
        command.CommandText =
            """
                    SELECT Id, Name, Url, Feed, Image, LastFetchedAt
                    FROM Sources
                    WHERE Id = @id;
            """;

        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            return Source.FromDb(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            );
        }

        return null;
    }

    public async Task<bool> IsFeedUniqueAsync(string feed)
    {
        await using var conn = new SqliteConnection(_config.ConnectionString);
        await conn.OpenAsync();

        await using var command = conn.CreateCommand();
        command.CommandText = @"SELECT COUNT(*) FROM Sources WHERE Feed = @feed;";

        command.Parameters.AddWithValue("@feed", feed);

        var count = await command.ExecuteScalarAsync();
        return count is long and 0;
    }

    /// <summary> Updates LastFetchedAt </summary>
    public async Task UpdateSourceAsync(Source source)
    {
        await using var conn = new SqliteConnection(_config.ConnectionString);
        await conn.OpenAsync();

        await using var command = conn.CreateCommand();
        command.CommandText =
            """
            UPDATE Sources
            SET LastFetchedAt = @lastFetchedAt
            WHERE Id = @id;
            """;

        command.Parameters.AddWithValue("@lastFetchedAt", source.LastFetchedAt);
        command.Parameters.AddWithValue("@id", source.Id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task RemoveSourceAsync(Source source)
    {
        await using var conn = new SqliteConnection(_config.ConnectionString);
        await conn.OpenAsync();

        await using var command = conn.CreateCommand();
        command.CommandText = @"DELETE FROM Sources WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", source.Id);
        await command.ExecuteNonQueryAsync();
    }


    /// <summary> Lists unsent posts for spec source </summary>
    public async Task<IReadOnlyList<Post>> ListUnsentPostsAsync(int sourceId)
    {
        await using var conn = new SqliteConnection(_config.ConnectionString);
        await conn.OpenAsync();

        await using var command = conn.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Title, Desc, Thumbnail, Link, PublishedAt, SentAt, SourceId
            FROM Posts
            WHERE SourceId = @sourceId AND SentAt IS NULL;
            """;

        command.Parameters.AddWithValue("@sourceId", sourceId);

        var posts = new List<Post>();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var titleOrd = reader.GetOrdinal("Title");
            var descOrd = reader.GetOrdinal("Desc");
            var thumbnailOrd = reader.GetOrdinal("Thumbnail");
            var linkOrd = reader.GetOrdinal("Link");
            var publishedAtOrd = reader.GetOrdinal("PublishedAt");
            var sentAtOrd = reader.GetOrdinal("SentAt");
            var sourceIdOrd = reader.GetOrdinal("SourceId");

            posts.Add(Post.FromDb(
                reader.GetString(titleOrd),
                reader.IsDBNull(descOrd) ? null : reader.GetString(descOrd),
                reader.IsDBNull(thumbnailOrd) ? null : reader.GetString(thumbnailOrd),
                reader.GetString(linkOrd),
                reader.IsDBNull(publishedAtOrd) ? null : reader.GetDateTime(publishedAtOrd),
                reader.IsDBNull(sentAtOrd) ? null : reader.GetDateTime(sentAtOrd),
                reader.GetInt32(sourceIdOrd)
            ));
        }

        return posts.AsReadOnly();
    }

    /// <summary> This inserts only new posts if they are not already in the db </summary>
    public async Task AddPostsIfNotExistsAsync(IEnumerable<Post> posts)
    {
        await using var conn = new SqliteConnection(_config.ConnectionString);
        await conn.OpenAsync();

        await using var command = conn.CreateCommand();
        command.CommandText =
            """
            INSERT OR IGNORE INTO Posts(Title, Desc, Thumbnail, Link, PublishedAt, SentAt, SourceId)
            VALUES (@title, @desc, @thumbnail, @link, @publishedAt, @sentAt, @sourceId);
            """;

        foreach (var post in posts)
        {
            command.Parameters.Clear();

            command.Parameters.AddWithValue("@title", post.Title);
            command.Parameters.AddWithValue("@desc", post.Desc);
            command.Parameters.AddWithValue("@thumbnail", post.Thumbnail);
            command.Parameters.AddWithValue("@link", post.Link);
            command.Parameters.AddWithValue("@publishedAt", post.PublishedAt);
            command.Parameters.AddWithValue("@sentAt", (object?)post.SentAt ?? DBNull.Value);
            command.Parameters.AddWithValue("@sourceId", post.SourceId);

            await command.ExecuteNonQueryAsync();
        }
    }

    /// <summary> Updates the SentAt if set, otherwise null </summary>
    public async Task UpdatePostsAsync(IEnumerable<Post> recent)
    {
        await using var conn = new SqliteConnection(_config.ConnectionString);
        await conn.OpenAsync();

        await using var command = conn.CreateCommand();
        command.CommandText =
            """
            UPDATE Posts
            SET SentAt = @sentAt
            WHERE SourceId = @sourceId AND Link = @link;
            """;

        foreach (var post in recent)
        {
            command.Parameters.Clear();

            command.Parameters.AddWithValue("@sentAt", (object?)post.SentAt ?? DBNull.Value);
            command.Parameters.AddWithValue("@sourceId", post.SourceId);
            command.Parameters.AddWithValue("@link", post.Link);

            await command.ExecuteNonQueryAsync();
        }
    }
}