namespace Raven.Core.Abstractions;

public interface IStore
{
    Task AddSourcesAsync(IEnumerable<Source> sources);
    
    Task<IReadOnlyList<Source>> ListSourcesAsync();
    
    Task<Source?> FindSourceByIdAsync(int id);

    Task<bool> IsFeedUniqueAsync(string feed);

    Task UpdateSourceAsync(Source source);

    Task RemoveSourceAsync(Source source);

    Task<IReadOnlyList<Post>> ListUnsentPostsAsync(int sourceId);

    Task AddPostsIfNotExistsAsync(IEnumerable<Post> posts);

    Task UpdatePostsAsync(IEnumerable<Post> recent);
}