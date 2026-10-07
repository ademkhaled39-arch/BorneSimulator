using System.Text.Json;
using Borne.Domain.Entities;
using Borne.Domain.IInfrastructure;

namespace Borne.Infrastructure.Data;

/// <summary>
/// Implementation of IConfigurationProvider that loads Borne configurations from JSON files.
/// </summary>
public class JsonConfigurationProvider : IConfigurationProvider
{
    private readonly string _configDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonConfigurationProvider(string configDirectory = "Configurations")
    {
        _configDirectory = configDirectory;

        // Ensure configuration directory exists
        if (!Directory.Exists(_configDirectory))
        {
            Directory.CreateDirectory(_configDirectory);
        }

        // Configure JSON serializer options for consistent formatting
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Loads configuration for a specific Borne by its ID.
    /// Looks for a JSON file named {borneId}.json in the configuration directory.
    /// </summary>
    public async Task<BorneConfig?> LoadConfigurationAsync(string borneId)
    {
        if (string.IsNullOrWhiteSpace(borneId))
            throw new ArgumentException("BorneId cannot be empty.", nameof(borneId));

        var configPath = Path.Combine(_configDirectory, $"{borneId}.json");
        return await LoadConfigurationFromFileAsync(configPath);
    }

    /// <summary>
    /// Loads all available Borne configurations from the configuration directory.
    /// </summary>
    public async Task<IEnumerable<BorneConfig>> LoadAllConfigurationsAsync()
    {
        var configs = new List<BorneConfig>();

        if (!Directory.Exists(_configDirectory))
            return configs;

        var jsonFiles = Directory.GetFiles(_configDirectory, "*.json");

        foreach (var file in jsonFiles)
        {
            try
            {
                var config = await LoadConfigurationFromFileAsync(file);
                if (config != null)
                {
                    configs.Add(config);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load configuration from {file}: {ex.Message}");
            }
        }

        return configs;
    }

    /// <summary>
    /// Loads configuration from a specific file path.
    /// </summary>
    public async Task<BorneConfig?> LoadConfigurationFromFileAsync(string configPath)
    {
        if (!File.Exists(configPath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(configPath);
            var configDto = JsonSerializer.Deserialize<BorneConfigDto>(json, _jsonOptions);

            if (configDto == null)
                return null;

            // Map DTO to domain entity with validation
            return BorneConfig.Create(
                borneId: configDto.BorneId,
                borneName: configDto.BorneName,
                brokerConfig: BrokerConfig.Create(
                    host: configDto.BrokerConfig.Host,
                    port: configDto.BrokerConfig.Port,
                    topicPrefix: configDto.BrokerConfig.TopicPrefix,
                    username: configDto.BrokerConfig.Username,
                    password: configDto.BrokerConfig.Password,
                    reconnectDelayMilliseconds: configDto.BrokerConfig.ReconnectDelayMilliseconds),
                databaseConfig: DatabaseConfig.Create(
                    connectionString: configDto.DatabaseConfig.ConnectionString,
                    databaseType: configDto.DatabaseConfig.DatabaseType,
                    connectionTimeoutSeconds: configDto.DatabaseConfig.ConnectionTimeoutSeconds,
                    maxPoolSize: configDto.DatabaseConfig.MaxPoolSize));
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse configuration file {configPath}. Invalid JSON format.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load configuration from {configPath}.", ex);
        }
    }

    /// <summary>
    /// DTO for JSON deserialization of Borne configuration.
    /// </summary>
    private class BorneConfigDto
    {
        public required string BorneId { get; set; }
        public required string BorneName { get; set; }
        public required BrokerConfigDto BrokerConfig { get; set; }
        public required DatabaseConfigDto DatabaseConfig { get; set; }
    }

    private class BrokerConfigDto
    {
        public required string Host { get; set; }
        public required int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public required string TopicPrefix { get; set; }
        public int ReconnectDelayMilliseconds { get; set; } = 5000;
    }

    private class DatabaseConfigDto
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseType { get; set; }
        public int ConnectionTimeoutSeconds { get; set; } = 30;
        public int MaxPoolSize { get; set; } = 10;
    }
}

/// <summary>
/// Implementation of IConfigurationRepository that persists Borne configurations to JSON files.
/// </summary>
public class JsonConfigurationRepository : IConfigurationRepository
{
    private readonly string _configDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonConfigurationRepository(string configDirectory = "Configurations")
    {
        _configDirectory = configDirectory;

        if (!Directory.Exists(_configDirectory))
        {
            Directory.CreateDirectory(_configDirectory);
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Saves a Borne configuration to a JSON file.
    /// </summary>
    public async Task<bool> SaveConfigurationAsync(BorneConfig config)
    {
        try
        {
            var configDto = new BorneConfigDto
            {
                BorneId = config.BorneId,
                BorneName = config.BorneName,
                BrokerConfig = new BrokerConfigDto
                {
                    Host = config.BrokerConfig.Host,
                    Port = config.BrokerConfig.Port,
                    Username = config.BrokerConfig.Username,
                    Password = config.BrokerConfig.Password,
                    TopicPrefix = config.BrokerConfig.TopicPrefix,
                    ReconnectDelayMilliseconds = config.BrokerConfig.ReconnectDelayMilliseconds
                },
                DatabaseConfig = new DatabaseConfigDto
                {
                    ConnectionString = config.DatabaseConfig.ConnectionString,
                    DatabaseType = config.DatabaseConfig.DatabaseType,
                    ConnectionTimeoutSeconds = config.DatabaseConfig.ConnectionTimeoutSeconds,
                    MaxPoolSize = config.DatabaseConfig.MaxPoolSize
                }
            };

            var filePath = Path.Combine(_configDirectory, $"{config.BorneId}.json");
            var json = JsonSerializer.Serialize(configDto, _jsonOptions);

            await File.WriteAllTextAsync(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Failed to save configuration for Borne {config.BorneId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Deletes a Borne configuration file.
    /// </summary>
    public async Task<bool> DeleteConfigurationAsync(string borneId)
    {
        try
        {
            var filePath = Path.Combine(_configDirectory, $"{borneId}.json");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Failed to delete configuration for Borne {borneId}: {ex.Message}");
            return await Task.FromResult(false);
        }
    }

    /// <summary>
    /// Checks if a configuration file exists for a Borne.
    /// </summary>
    public async Task<bool> ConfigurationExistsAsync(string borneId)
    {
        var filePath = Path.Combine(_configDirectory, $"{borneId}.json");
        return await Task.FromResult(File.Exists(filePath));
    }

    /// <summary>
    /// DTO classes for JSON serialization/deserialization.
    /// </summary>
    private class BorneConfigDto
    {
        public required string BorneId { get; set; }
        public required string BorneName { get; set; }
        public required BrokerConfigDto BrokerConfig { get; set; }
        public required DatabaseConfigDto DatabaseConfig { get; set; }
    }

    private class BrokerConfigDto
    {
        public required string Host { get; set; }
        public required int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public required string TopicPrefix { get; set; }
        public int ReconnectDelayMilliseconds { get; set; } = 5000;
    }

    private class DatabaseConfigDto
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseType { get; set; }
        public int ConnectionTimeoutSeconds { get; set; } = 30;
        public int MaxPoolSize { get; set; } = 10;
    }
}
