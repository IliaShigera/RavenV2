namespace Raven.Core.Telegram;

public sealed class TelegramClient
{
    private readonly HttpClient _http;
    private readonly TelegramConfiguration _config;

    public TelegramClient(HttpClient http, TelegramConfiguration config)
    {
        _http = http;
        _config = config;
    }

    private const string BaseUrl = "https://api.telegram.org";

    private const int MaxRetries = 3;
    private const int RetryDelaySeconds = 5;
    private int _currentRetryCount = 0;

    public async Task<Result<Empty>> SendPostAsync(TelegramPost post)
    {
        ArgumentNullException.ThrowIfNull(post);

        try
        {
            var url = $"{BaseUrl}/bot{_config.Token}/sendPhoto";

            var payload = new
            {
                chat_id = _config.ChatId,
                photo = post.PreviewImageUrl,
                caption = post.Content,
                parse_mode = "HTML",
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);
            // response.EnsureSuccessStatusCode();
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<Empty>.Failure(error);
            }

            return Result<Empty>.Success();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            // todo: is there retry after in response ? 

            await Task.Delay(TimeSpan.FromSeconds(10));
            await SendPostAsync(post);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return Result<Empty>.Failure(
                $"{ex.Message}. Check if the bot is added to the group and has permission to send messages.");
        }
        catch (HttpRequestException ex)
        {
            _currentRetryCount++;

            if (_currentRetryCount > MaxRetries)
            {
                return Result<Empty>.Failure($"Retry count exceeded. Last error: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(_currentRetryCount * RetryDelaySeconds));
            return await SendPostAsync(post);
        }
        catch (Exception ex)
        {
            return Result<Empty>.Failure(ex.Message);
        }
        finally
        {
            _currentRetryCount = 0;
        }

        return Result<Empty>.Failure("Unknown error");
    }
}