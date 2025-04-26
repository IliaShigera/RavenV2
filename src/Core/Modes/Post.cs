namespace Raven.Core.Modes;

public sealed class Post
{
    private Post(string title, string? desc, string? thumbnail, string link, DateTime? publishedAt, int sourceId)
    {
        Title = title;
        Desc = desc;
        Thumbnail = thumbnail;
        Link = link;
        PublishedAt = publishedAt;
        SourceId = sourceId;
    }

    public int Id { get; private set; }
    public string Title { get; private set; }
    public string? Desc { get; private set; }
    public string? Thumbnail { get; private set; }
    public string Link { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public int SourceId { get; private set; }

    public static Post Create(string title, string? desc, string? thumbnail, string link, DateTime? publishedAt, int sourceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(link, nameof(link));

        return new Post(title, desc, thumbnail, link, publishedAt, sourceId);
    }

    public void MarkAsSent() => SentAt = DateTime.UtcNow;

    public static Post FromDb(string title, string? desc, string? thumbnail, string link, DateTime? publishedAt, DateTime ?sentAt, int sourceId)
    {
        return new Post(title, desc, thumbnail, link, publishedAt, sourceId) { SourceId = sourceId, SentAt = sentAt };
    }
}