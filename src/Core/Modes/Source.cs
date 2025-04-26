namespace Raven.Core.Modes;

public sealed class Source
{
    [JsonConstructor]
    private Source(string name, string url, string feed, string? image)
    {
        Name = name;
        Url = url;
        Feed = feed;
        Image = image;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Url { get; private set; }
    public string Feed { get; private set; }

    /// <summary> Used for fallback if a preview image is not found </summary>
    public string? Image { get; private set; }

    /// <summary> Last successful fetch (with no error) </summary>
    public DateTime? LastFetchedAt { get; private set; }


    public static Source Create(string name, string url, string feed, string? image = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));
        ArgumentException.ThrowIfNullOrWhiteSpace(feed, nameof(feed));

        return new Source(name, url, feed, image);
    }

    public static Source FromDb(int id, string name, string url, string feed, string? image, DateTime? lastFetchedAt)
    {
        return new Source(name, url, feed, image) { Id = id, LastFetchedAt = lastFetchedAt };
    }

    public void UpdateLastFetchedAt() => LastFetchedAt = DateTime.UtcNow;
}