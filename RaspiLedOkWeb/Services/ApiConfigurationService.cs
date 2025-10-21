using JsonDataClass;
using Microsoft.Extensions.Options;
using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using System.Text.Json;

namespace RaspiLedOkWeb.Services
{
    public class ApiConfigurationService(
        IOptionsMonitor<ApiConfiguration> options,
        IConfiguration configuration,
        ILogger<ApiConfigurationService> logger,
        IWebHostEnvironment environment) : IApiConfigurationService
    {
        #region Properties
        private readonly IOptionsMonitor<ApiConfiguration> _options = options;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<ApiConfigurationService> _logger = logger;
        private readonly IWebHostEnvironment _environment = environment;
        private ApiConfiguration? _cachedConfiguration;
        #endregion

        #region Screen Configuration

        #endregion

        #region Api Configuration
        public ApiConfiguration GetConfiguration()
        {
            // Always get the current value to ensure we have the latest configuration
            _cachedConfiguration = _options.CurrentValue;
            _logger.LogInformation("API configuration loaded: Endpoint={Endpoint}", 
                _cachedConfiguration.ApiUrl);
            
            return _cachedConfiguration;
        }

        public async Task UpdateConfigurationAsync(ApiConfiguration configuration)
        {
            if (!ValidateConfiguration(configuration))
            {
                throw new ArgumentException("Invalid configuration provided");
            }

            // Update the cached configuration
            _cachedConfiguration = configuration;
            
            // Update the appsettings.json file
            await UpdateAppSettingsFileAsync(configuration);
            
            _logger.LogInformation("API configuration updated and persisted: Endpoint={Endpoint}", 
                configuration.ApiUrl);
        }

        public bool ValidateConfiguration(ApiConfiguration configuration)
        {
            if (configuration == null)
            {
                _logger.LogWarning("Configuration is null");
                return false;
            }

            if (string.IsNullOrWhiteSpace(configuration.ApiUrl))
            {
                _logger.LogWarning("API endpoint is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(configuration.Username))
            {
                _logger.LogWarning("API Key (Username) is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(configuration.Password))
            {
                _logger.LogWarning("API Key (Password) is required");
                return false;
            }

            if (!Uri.TryCreate(configuration.ApiUrl, UriKind.Absolute, out _))
            {
                _logger.LogWarning("Invalid API endpoint URL format");
                return false;
            }

            if (configuration.TimeoutSeconds <= 0 || configuration.TimeoutSeconds > 300)
            {
                _logger.LogWarning("Timeout must be between 1 and 300 seconds");
                return false;
            }

            return true;
        }
        #endregion

        #region Assets Configuration
        public List<Asset> GetAssets()
        {
            try
            {
                var assetsSection = _configuration.GetSection("Assets");
                var assets = assetsSection.Get<List<Asset>>();
                return assets ?? new List<Asset>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading assets from configuration");
                return new List<Asset>();
            }
        }

        public async Task UpdateAssetsAsync(ConfigurationAssets configurationAssets)
        {
            try
            {
                await UpdateSectionInSettingsFileAsync("Assets", configurationAssets.Assets);
                _logger.LogInformation("Assets updated successfully. Count: {Count}", configurationAssets.Assets.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assets");
                throw;
            }
        }
        #endregion

        #region AppSettings
        public AppSettings GetAppSettings()
        {
            try
            {
                var appSettingsSection = _configuration.GetSection("AppSettings");
                var appSettings = appSettingsSection.Get<AppSettings>();
                return appSettings ?? new AppSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading app settingg from configuration");
                return new AppSettings();
            }
        }

        #endregion

        #region Helpers
        private async Task UpdateAppSettingsFileAsync(ApiConfiguration configuration)
        {
            try
            {
                // Determine which appsettings file to update based on environment
                var fileName = _environment.IsDevelopment()
                    ? "appsettings.Development.json"
                    : "appsettings.json";

                var filePath = Path.Combine(_environment.ContentRootPath, fileName);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Settings file not found: {FilePath}", filePath);
                    return;
                }

                // Read the current settings file
                var jsonString = await File.ReadAllTextAsync(filePath);

                // Parse as JsonNode for easier manipulation while preserving structure
                using var jsonDocument = JsonDocument.Parse(jsonString);
                var settingsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonDocument.RootElement);

                if (settingsDict == null)
                {
                    _logger.LogError("Failed to parse settings file");
                    return;
                }

                // Update the ApiConfiguration section
                var apiConfigDict = new Dictionary<string, object>
                {
                    ["ApiUrl"] = configuration.ApiUrl,
                    ["Username"] = configuration.Username,
                    ["Password"] = configuration.Password,
                    ["TimeoutSeconds"] = configuration.TimeoutSeconds,
                    ["EnableLogging"] = configuration.EnableLogging,
                    ["Screen"] = JsonSerializer.Serialize(configuration.Screen)
                };

                settingsDict[ApiConfiguration.SectionName] = apiConfigDict;

                // Write back to file with proper formatting
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var updatedJson = JsonSerializer.Serialize(settingsDict, options);
                await File.WriteAllTextAsync(filePath, updatedJson);

                _logger.LogInformation("Settings file updated: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update settings file");
                throw;
            }
        }

        private async Task UpdateSectionInSettingsFileAsync<T>(string sectionName, T data)
        {
            try
            {
                // Determine which appsettings file to update based on environment
                var fileName = _environment.IsDevelopment()
                    ? "appsettings.Development.json"
                    : "appsettings.json";

                var filePath = Path.Combine(_environment.ContentRootPath, fileName);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Settings file not found: {FilePath}", filePath);
                    return;
                }

                // Read the current settings file
                var jsonString = await File.ReadAllTextAsync(filePath);

                using var jsonDocument = JsonDocument.Parse(jsonString);
                var settingsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonDocument.RootElement);

                if (settingsDict == null)
                {
                    _logger.LogError("Failed to parse settings file");
                    return;
                }

                // Update the specified section
                settingsDict[sectionName] = data;

                // Write back to file with proper formatting
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var updatedJson = JsonSerializer.Serialize(settingsDict, options);
                await File.WriteAllTextAsync(filePath, updatedJson);

                _logger.LogInformation("Settings section {SectionName} updated in file: {FilePath}", sectionName, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update settings section {SectionName}", sectionName);
                throw;
            }
        }
        #endregion
    }
}
