namespace Raven.Core.Configuration;

public sealed class TelegramPostSettings
{
    public TelegramPostSettings(string defaultPreviewImage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultPreviewImage, nameof(defaultPreviewImage));

        DefaultPreviewImage = defaultPreviewImage;
    }

    public string DefaultPreviewImage { get; }
}