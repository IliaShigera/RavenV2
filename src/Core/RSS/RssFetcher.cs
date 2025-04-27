namespace Raven.Core.RSS;

public sealed class RssFetcher
{
    private readonly HttpClient _http;

    public RssFetcher(HttpClient http)
    {
        _http = http;
    }

    public async Task<Result<IReadOnlyList<Post>>> FetchAsync(Source source)
    {
        try
        {
            var response = await _http.GetStringAsync(source.Feed);
            using var reader = XmlReader.Create(new StringReader(response));
            var feed = SyndicationFeed.Load(reader);

            var posts = feed.Items
                .Select(item => Post.Create(
                    title: item.Title?.Text ?? source.Name,
                    desc: item.Summary?.Text,
                    thumbnail: TryExtractPostPreviewImage(item) ?? feed.ImageUrl?.ToString() ?? null,
                    link: item.Links.FirstOrDefault()?.Uri.ToString()!,
                    publishedAt: item.PublishDate.UtcDateTime,
                    sourceId: source.Id
                ))
                .ToList();

            return Result<IReadOnlyList<Post>>.Success(posts);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Post>>.Failure(ex.Message);
        }
    }


    private static string? TryExtractPostPreviewImage(SyndicationItem item)
    {
        // YT - check <media:thumbnail> in <media:group>
        var mediaGroup = item.ElementExtensions
            .FirstOrDefault(e => e.OuterName == "group" && e.OuterNamespace == "http://search.yahoo.com/mrss/");

        if (mediaGroup is not null)
        {
            using var reader = mediaGroup.GetReader();
            while (reader.Read())
            {
                if (reader is
                    {
                        NodeType: XmlNodeType.Element,
                        LocalName: "thumbnail",
                        NamespaceURI: "http://search.yahoo.com/mrss/"
                    })
                {
                    var url = reader.GetAttribute("url");
                    if (!string.IsNullOrWhiteSpace(url))
                        return url;
                }
            }
        }

        // Blogs - check <itunes:image>  
        var itunesImage = item.ElementExtensions
            .FirstOrDefault(e =>
                e.OuterName == "image" && e.OuterNamespace == "http://www.itunes.com/dtds/podcast-1.0.dtd");

        if (itunesImage is not null)
            return itunesImage.GetReader().GetAttribute("href");

        // Also Blogs - check <media:content> with image type
        var mediaContent = item.ElementExtensions
            .FirstOrDefault(e =>
                e.OuterName == "content" &&
                e.OuterNamespace == "http://search.yahoo.com/mrss/" &&
                e.GetReader().GetAttribute("medium") == "image");

        if (mediaContent is not null)
            return mediaContent.GetReader().GetAttribute("url");

        // Fallback - check <enclosure> for image types
        var enclosure = item.Links
            .FirstOrDefault(l => l.RelationshipType == "enclosure" &&
                                 l.MediaType?.StartsWith("image/") == true);

        if (enclosure is not null)
            return enclosure.Uri?.ToString();

        // idk, some edge case
        // var customImage = item.ElementExtensions
        //     .FirstOrDefault(e => e.OuterName == "image" &&
        //                          (e.OuterNamespace == "http://purl.org/dc/elements/1.1/" ||
        //                           e.OuterNamespace == "http://ogp.me/ns#"));
        //
        // if (customImage is not null)
        //     return customImage.GetReader().ReadElementContentAsString();

        // No preview image 
        return null;
    }
}