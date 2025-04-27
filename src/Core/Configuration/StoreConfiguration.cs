namespace Raven.Core.Configuration;

public sealed class StoreConfiguration
{
    public StoreConfiguration(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));
        
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }
}