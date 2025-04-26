namespace Raven.Core.Telegram;

public sealed class TelegramPost
{
    internal TelegramPost(string content, string? previewImageUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content, nameof(content));

        Content = content;
        PreviewImageUrl = previewImageUrl;
    }

    public string Content { get; }
    public string? PreviewImageUrl { get; }
}