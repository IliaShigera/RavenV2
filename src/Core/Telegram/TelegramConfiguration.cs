namespace Raven.Core.Telegram;

public sealed class TelegramConfiguration
{
    public TelegramConfiguration(string token, string chatId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));
        ArgumentException.ThrowIfNullOrWhiteSpace(chatId, nameof(chatId));

        Token = token;
        ChatId = chatId;
    }

    public string Token { get; }
    public string ChatId { get; }
}