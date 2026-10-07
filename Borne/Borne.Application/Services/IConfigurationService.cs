using Borne.Domain.Entities;

namespace Borne.Application.Services;

/// <summary>
/// Application service interface for loading and managing this Borne instance's configuration.
/// Responsible for loading configuration from various sources (JSON files, environment variables, etc.)
/// and initializing the Borne with its settings.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Loads and initializes this Borne's configuration by ID.
    /// Loads configuration from file, validates it, and prepares it for use.
    /// </summary>
    /// <param name="borneId">The unique identifier of this Borne instance</param>
    /// <returns>The loaded and validated BorneConfig</returns>
    Task<BorneConfig> InitializeBorneConfigAsync(string borneId);

    /// <summary>
    /// Loads this Borne's configuration from environment variables.
    /// Environment variables should include:
    /// - BORNE_ID
    /// - BORNE_NAME
    /// - BROKER_HOST
    /// - BROKER_PORT
    /// - BROKER_TOPIC_PREFIX
    /// - DATABASE_CONNECTION_STRING
    /// - DATABASE_TYPE
    /// </summary>
    /// <returns>The BorneConfig loaded from environment variables</returns>
    Task<BorneConfig> LoadConfigurationFromEnvironmentAsync();

    /// <summary>
    /// Loads configuration from a specific configuration file.
    /// </summary>
    /// <param name="configPath">Path to the configuration JSON file</param>
    /// <returns>The loaded BorneConfig</returns>
    Task<BorneConfig> LoadConfigurationFromFileAsync(string configPath);
}
