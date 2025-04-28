namespace Raven.Core.Telegram;

public sealed class TelegramPostFactory
{
    private const int MaxDescLength = 625;

    private readonly TelegramPostSettings _settings;

    public TelegramPostFactory(TelegramPostSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _settings = settings;
    }

    public TelegramPost CreateFrom(Source source, Post post)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(post, nameof(post));

        var pubDate = post.PublishedAt?.ToString("MMMM dd, yyyy") ?? DateTime.Now.ToString("MMMM dd, yyyy");
        var preview = post.Thumbnail is not null && Uri.TryCreate(post.Thumbnail, UriKind.Absolute, out var uri)
            ? uri.AbsoluteUri
            : source.Image ?? _settings.DefaultPreviewImage;

        var sb = new StringBuilder();

        sb.AppendLine($"<b>{post.Title}</b>");
        sb.AppendLine();

        sb.AppendLine($"<a href=\"{EscapeHtml(source.Url)}\">{EscapeHtml(source.Name)}</a> | {pubDate}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(post.Desc))
        {
            var decoded = WebUtility.HtmlDecode(post.Desc);
            var plainText = Regex.Replace(decoded, "<.*?>", string.Empty);

            if (plainText.Length > MaxDescLength)
            {
                var safeStart = Math.Min(MaxDescLength, plainText.Length - 1);
                var lastSpace = plainText.LastIndexOf(' ', safeStart);
                if (lastSpace > 0) sb.AppendLine(plainText[..lastSpace] + "...");
                else sb.AppendLine(plainText[..safeStart] + "...");
            }
            else
                sb.AppendLine(plainText);

            sb.AppendLine();
        }

        sb.AppendLine(EscapeHtml(post.Link));

        return new TelegramPost(sb.ToString(), preview);
    }


    private static string EscapeHtml(string input) =>
        input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
}