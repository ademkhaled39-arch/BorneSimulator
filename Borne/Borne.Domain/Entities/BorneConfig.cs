namespace Borne.Domain.Entities;

/// <summary>
/// Represents the configuration for a Borne instance.
/// Each Borne has its own configuration including ID, database connection, and broker communication settings.
/// </summary>
public class BorneConfig
{
    public required string BorneId { get; set; }
    public required string BorneName { get; set; }
    public required BrokerConfig BrokerConfig { get; set; }
    public required DatabaseConfig DatabaseConfig { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    /// <summary>
    /// Creates a new BorneConfig instance with validation.
    /// </summary>
    public static BorneConfig Create(
        string borneId,
        string borneName,
        BrokerConfig brokerConfig,
        DatabaseConfig databaseConfig)
    {
        if (string.IsNullOrWhiteSpace(borneId))
            throw new ArgumentException("BorneId cannot be empty.", nameof(borneId));

        if (string.IsNullOrWhiteSpace(borneName))
            throw new ArgumentException("BorneName cannot be empty.", nameof(borneName));

        if (brokerConfig == null)
            throw new ArgumentNullException(nameof(brokerConfig));

        if (databaseConfig == null)
            throw new ArgumentNullException(nameof(databaseConfig));

        return new BorneConfig
        {
            BorneId = borneId,
            BorneName = borneName,
            BrokerConfig = brokerConfig,
            DatabaseConfig = databaseConfig,
            CreatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Represents the broker communication configuration for a Borne.
/// </summary>
public class BrokerConfig
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public required string TopicPrefix { get; set; }
    public int ReconnectDelayMilliseconds { get; set; } = 5000;

    /// <summary>
    /// Creates a new BrokerConfig instance with validation.
    /// </summary>
    public static BrokerConfig Create(
        string host,
        int port,
        string topicPrefix,
        string? username = null,
        string? password = null,
        int reconnectDelayMilliseconds = 5000)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be empty.", nameof(host));

        if (port < 1 || port > 65535)
            throw new ArgumentException("Port must be between 1 and 65535.", nameof(port));

        if (string.IsNullOrWhiteSpace(topicPrefix))
            throw new ArgumentException("TopicPrefix cannot be empty.", nameof(topicPrefix));

        if (reconnectDelayMilliseconds < 0)
            throw new ArgumentException("ReconnectDelayMilliseconds cannot be negative.", nameof(reconnectDelayMilliseconds));

        return new BrokerConfig
        {
            Host = host,
            Port = port,
            TopicPrefix = topicPrefix,
            Username = username,
            Password = password,
            ReconnectDelayMilliseconds = reconnectDelayMilliseconds
        };
    }
}

/// <summary>
/// Represents the database configuration for a Borne.
/// </summary>
public class DatabaseConfig
{
    public required string ConnectionString { get; set; }
    public required string DatabaseType { get; set; } // e.g., "SqlServer", "PostgreSQL", "SQLite"
    public int ConnectionTimeoutSeconds { get; set; } = 30;
    public int MaxPoolSize { get; set; } = 10;

    /// <summary>
    /// Creates a new DatabaseConfig instance with validation.
    /// </summary>
    public static DatabaseConfig Create(
        string connectionString,
        string databaseType,
        int connectionTimeoutSeconds = 30,
        int maxPoolSize = 10)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("ConnectionString cannot be empty.", nameof(connectionString));

        if (string.IsNullOrWhiteSpace(databaseType))
            throw new ArgumentException("DatabaseType cannot be empty.", nameof(databaseType));

        if (connectionTimeoutSeconds < 1)
            throw new ArgumentException("ConnectionTimeoutSeconds must be at least 1.", nameof(connectionTimeoutSeconds));

        if (maxPoolSize < 1)
            throw new ArgumentException("MaxPoolSize must be at least 1.", nameof(maxPoolSize));

        return new DatabaseConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType,
            ConnectionTimeoutSeconds = connectionTimeoutSeconds,
            MaxPoolSize = maxPoolSize
        };
    }
}
