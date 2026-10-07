using Borne.Domain.Entities;
using Borne.Domain.IInfrastructure;

namespace Borne.Application.Services;

/// <summary>
/// Implementation of IConfigurationService that manages this Borne instance's configuration.
/// Loads configuration from files or environment variables.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationProvider _configProvider;

    public ConfigurationService(IConfigurationProvider configProvider)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
    }

    /// <summary>
    /// Loads and initializes this Borne's configuration by ID.
    /// </summary>
    public async Task<BorneConfig> InitializeBorneConfigAsync(string borneId)
    {
        if (string.IsNullOrWhiteSpace(borneId))
            throw new ArgumentException("BorneId cannot be empty.", nameof(borneId));

        var config = await _configProvider.LoadConfigurationAsync(borneId);

        if (config == null)
            throw new InvalidOperationException($"Configuration not found for Borne '{borneId}'.");

        return config;
    }

    /// <summary>
    /// Loads configuration from environment variables.
    /// Supports the following variables:
    /// - BORNE_ID (required)
    /// - BORNE_NAME (required)
    /// - BROKER_HOST (required)
    /// - BROKER_PORT (required)
    /// - BROKER_TOPIC_PREFIX (required)
    /// - BROKER_USERNAME (optional)
    /// - BROKER_PASSWORD (optional)
    /// - BROKER_RECONNECT_DELAY_MS (optional, default: 5000)
    /// - DATABASE_CONNECTION_STRING (required)
    /// - DATABASE_TYPE (required)
    /// - DATABASE_TIMEOUT_SECONDS (optional, default: 30)
    /// - DATABASE_MAX_POOL_SIZE (optional, default: 10)
    /// </summary>
    public async Task<BorneConfig> LoadConfigurationFromEnvironmentAsync()
    {
        try
        {
            // Load required Borne settings
            var borneId = Environment.GetEnvironmentVariable("BORNE_ID");
            if (string.IsNullOrWhiteSpace(borneId))
                throw new InvalidOperationException("Environment variable 'BORNE_ID' is not set.");

            var borneName = Environment.GetEnvironmentVariable("BORNE_NAME");
            if (string.IsNullOrWhiteSpace(borneName))
                throw new InvalidOperationException("Environment variable 'BORNE_NAME' is not set.");

            // Load Broker settings
            var brokerHost = Environment.GetEnvironmentVariable("BROKER_HOST");
            if (string.IsNullOrWhiteSpace(brokerHost))
                throw new InvalidOperationException("Environment variable 'BROKER_HOST' is not set.");

            if (!int.TryParse(Environment.GetEnvironmentVariable("BROKER_PORT"), out var brokerPort))
                throw new InvalidOperationException("Environment variable 'BROKER_PORT' must be a valid integer.");

            var brokerTopicPrefix = Environment.GetEnvironmentVariable("BROKER_TOPIC_PREFIX");
            if (string.IsNullOrWhiteSpace(brokerTopicPrefix))
                throw new InvalidOperationException("Environment variable 'BROKER_TOPIC_PREFIX' is not set.");

            var brokerUsername = Environment.GetEnvironmentVariable("BROKER_USERNAME");
            var brokerPassword = Environment.GetEnvironmentVariable("BROKER_PASSWORD");

            if (!int.TryParse(
                Environment.GetEnvironmentVariable("BROKER_RECONNECT_DELAY_MS") ?? "5000",
                out var reconnectDelay))
            {
                reconnectDelay = 5000;
            }

            // Load Database settings
            var dbConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(dbConnectionString))
                throw new InvalidOperationException("Environment variable 'DATABASE_CONNECTION_STRING' is not set.");

            var dbType = Environment.GetEnvironmentVariable("DATABASE_TYPE");
            if (string.IsNullOrWhiteSpace(dbType))
                throw new InvalidOperationException("Environment variable 'DATABASE_TYPE' is not set.");

            if (!int.TryParse(
                Environment.GetEnvironmentVariable("DATABASE_TIMEOUT_SECONDS") ?? "30",
                out var dbTimeout))
            {
                dbTimeout = 30;
            }

            if (!int.TryParse(
                Environment.GetEnvironmentVariable("DATABASE_MAX_POOL_SIZE") ?? "10",
                out var dbPoolSize))
            {
                dbPoolSize = 10;
            }

            // Create configuration entities with validation
            var brokerConfig = BrokerConfig.Create(
                host: brokerHost,
                port: brokerPort,
                topicPrefix: brokerTopicPrefix,
                username: brokerUsername,
                password: brokerPassword,
                reconnectDelayMilliseconds: reconnectDelay);

            var databaseConfig = DatabaseConfig.Create(
                connectionString: dbConnectionString,
                databaseType: dbType,
                connectionTimeoutSeconds: dbTimeout,
                maxPoolSize: dbPoolSize);

            var config = BorneConfig.Create(
                borneId: borneId,
                borneName: borneName,
                brokerConfig: brokerConfig,
                databaseConfig: databaseConfig);

            return await Task.FromResult(config);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load configuration from environment variables.", ex);
        }
    }

    /// <summary>
    /// Loads configuration from a specific JSON file.
    /// </summary>
    public async Task<BorneConfig> LoadConfigurationFromFileAsync(string configPath)
    {
        if (string.IsNullOrWhiteSpace(configPath))
            throw new ArgumentException("Configuration path cannot be empty.", nameof(configPath));

        var config = await _configProvider.LoadConfigurationFromFileAsync(configPath);

        if (config == null)
            throw new InvalidOperationException($"Configuration file not found at '{configPath}'.");

        return config;
    }
}
