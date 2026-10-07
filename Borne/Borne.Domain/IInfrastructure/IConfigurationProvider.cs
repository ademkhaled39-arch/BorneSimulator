using Borne.Domain.Entities;

namespace Borne.Domain.IInfrastructure;

/// <summary>
/// Interface for loading Borne configuration from various sources.
/// Implementations can load from files, databases, or other configuration stores.
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// Loads configuration for a specific Borne by its ID.
    /// </summary>
    /// <param name="borneId">The unique identifier of the Borne</param>
    /// <returns>The BorneConfig if found; null if not found</returns>
    Task<BorneConfig?> LoadConfigurationAsync(string borneId);

    /// <summary>
    /// Loads all available Borne configurations.
    /// </summary>
    /// <returns>Collection of all available BorneConfigs</returns>
    Task<IEnumerable<BorneConfig>> LoadAllConfigurationsAsync();

    /// <summary>
    /// Loads configuration by configuration file path or name.
    /// </summary>
    /// <param name="configPath">Path to the configuration file</param>
    /// <returns>The BorneConfig if successfully loaded</returns>
    Task<BorneConfig?> LoadConfigurationFromFileAsync(string configPath);
}

/// <summary>
/// Interface for persisting and managing Borne configurations.
/// </summary>
public interface IConfigurationRepository
{
    /// <summary>
    /// Saves a Borne configuration.
    /// </summary>
    /// <param name="config">The BorneConfig to save</param>
    /// <returns>True if successful; false otherwise</returns>
    Task<bool> SaveConfigurationAsync(BorneConfig config);

    /// <summary>
    /// Deletes a Borne configuration by ID.
    /// </summary>
    /// <param name="borneId">The unique identifier of the Borne</param>
    /// <returns>True if successful; false otherwise</returns>
    Task<bool> DeleteConfigurationAsync(string borneId);

    /// <summary>
    /// Checks if a configuration exists for a specific Borne.
    /// </summary>
    /// <param name="borneId">The unique identifier of the Borne</param>
    /// <returns>True if configuration exists; false otherwise</returns>
    Task<bool> ConfigurationExistsAsync(string borneId);
}
