namespace Raven.Core.Abstractions;

public interface IDbInitializer
{
    Task InitializeAsync();
}