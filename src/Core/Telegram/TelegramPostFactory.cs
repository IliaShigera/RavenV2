namespace Raven.Core.Telegram;

public sealed class TelegramPostFactory
{
    private const int MaxDescLength = 625;
    
    private readonly string _defaultPreviewImageUrl;

    public TelegramPostFactory(string defaultPreviewImageUrl)
    {
        ArgumentNullException.ThrowIfNull(defaultPreviewImageUrl);

        _defaultPreviewImageUrl = defaultPreviewImageUrl;
    }

    public TelegramPost CreateFrom(Source source, Post post)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(post, nameof(post));

        var pubDate = post.PublishedAt?.ToString("MMMM dd, yyyy") ?? DateTime.Now.ToString("MMMM dd, yyyy");
        var previewImageUrl = post.Thumbnail ?? source.Image ?? _defaultPreviewImageUrl;
        var sb = new StringBuilder();

        sb.AppendLine($"<b>{post.Title}</b>");
        sb.AppendLine();

        sb.AppendLine($"<a href=\"{EscapeHtml(source.Url)}\">{EscapeHtml(source.Name)}</a> | {pubDate}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(post.Desc))
        {
            if (post.Desc.Length > MaxDescLength)
            {
                var raw = WebUtility.HtmlDecode(post.Desc);
                var lastSpace = raw.LastIndexOf(' ', MaxDescLength);
                if (lastSpace > 0)
                    sb.AppendLine(raw[..lastSpace] + "...");
            }
            else
                sb.AppendLine(EscapeHtml(post.Desc));

            sb.AppendLine();
        }

        sb.AppendLine(EscapeHtml(post.Link));

        return new TelegramPost(sb.ToString(), previewImageUrl);
    }


    private static string EscapeHtml(string input) =>
        input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
}