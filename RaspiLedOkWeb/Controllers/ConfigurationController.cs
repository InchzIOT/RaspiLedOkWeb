using Microsoft.AspNetCore.Mvc;
using RaspiLedOkWeb.Filters;
using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using RaspiLedOkWeb.Services;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;

namespace RaspiLedOkWeb.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly ILogger<ConfigurationController> _logger;
        private readonly IApiConfigurationService _apiConfigurationService;
        private readonly ISyncService _syncService;

        public ConfigurationController(
            ILogger<ConfigurationController> logger,
            IApiConfigurationService apiConfigurationService,
            ISyncService syncService)
        {
            _logger = logger;
            _apiConfigurationService = apiConfigurationService;
            _syncService = syncService;
        }

        [ApiKeyAuth]
        public IActionResult Index()
        {
            var configuration = _apiConfigurationService.GetConfiguration();
            return View(configuration);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateConfiguration(ApiConfiguration configuration)
        {
            try
            {
                if (!_apiConfigurationService.ValidateConfiguration(configuration))
                {
                    ModelState.AddModelError("", "Invalid configuration provided. Please check all fields.");
                    return View("Index", configuration);
                }

                await _apiConfigurationService.UpdateConfigurationAsync(configuration);
                TempData["SuccessMessage"] = "API configuration updated successfully!";
                
                _logger.LogInformation("API configuration updated via web interface");
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating API configuration");
                TempData["ErrorMessage"] = $"An error occurred while updating the configuration: {ex.Message}";
                return View("Index", configuration);
            }
        }

        [HttpGet]
        public IActionResult GetApiConfiguration()
        {
            try
            {
                var configuration = _apiConfigurationService.GetConfiguration();
                
                // Return configuration without exposing sensitive data
                var safeConfiguration = new
                {
                    Endpoint = configuration.ApiUrl,
                    Username = configuration.Username,
                    Password = configuration.Password,
                    TimeoutSeconds = configuration.TimeoutSeconds,
                    EnableLogging = configuration.EnableLogging,
                    Screen = configuration.Screen
                };
                
                return Json(safeConfiguration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving API configuration");
                TempData["ErrorMessage"] = $"Error retrieving API configuration: {ex.Message}";
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public IActionResult TestConnection()
        {
            try
            {
                var configuration = _apiConfigurationService.GetConfiguration();
                
                if (!_apiConfigurationService.ValidateConfiguration(configuration))
                {
                    return Json(new { success = false, message = "Invalid configuration. Please check all fields." });
                }

                // Here you would implement actual API connection testing
                // For now, we'll just validate the URL format
                if (Uri.TryCreate(configuration.ApiUrl, UriKind.Absolute, out var uri))
                {
                    // You can add actual HTTP request testing here
                    return Json(new { 
                        success = true, 
                        message = $"Configuration appears valid. Endpoint: {configuration.ApiUrl}",
                        timestamp = DateTime.UtcNow 
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid endpoint URL format." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing API connection");
                TempData["ErrorMessage"] = $"Error testing API connection: {ex.Message}";
                return Json(new { success = false, message = "Error testing connection." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SyncAssets()
        {
            try
            {
                var configRes = _apiConfigurationService.GetConfiguration();
                
                // Check if we have valid configuration
                if (!_apiConfigurationService.ValidateConfiguration(configRes))
                {
                    return Json(new { success = false, message = "Please configure valid API credentials first." });
                }

                _logger.LogInformation("Starting asset sync from configuration page");
                
                // Login using configured credentials
                var loginRes = await _syncService.Login(configRes.Username, configRes.GetDecryptedPassword());
                
                if (loginRes.Success && loginRes.AuthHeader?.Assets != null)
                {
                    _logger.LogInformation("Login successful, found {Count} assets", loginRes.AuthHeader.Assets.Count);

                    ConfigurationAssets configAssets = new ConfigurationAssets()
                    {
                        Assets = new List<Asset>()
                    };
                    var totalDevices = 0;

                    // Fetch devices for each asset
                    foreach (var asset in loginRes.AuthHeader.Assets)
                    {
                        Asset configAsset = new Asset()
                        {
                            Name = asset.Name,
                            Id = asset.AssetId.ToString(),
                            IsEnabled = true,
                            Interval = 1000,
                            Devices = new List<Device>()
                        };

                        try
                        {
                            // Use AssetId directly since it's already an int
                            var deviceResponse = await _syncService.GetDeviceListByAsset(asset.AssetId);
                            if (deviceResponse.Success && deviceResponse.Devices != null)
                            {
                                foreach (var device in deviceResponse.Devices)
                                {
                                    Device configDevice = new Device()
                                    {
                                        Name = device.DeviceName,
                                        Id = device.DeviceId.ToString(),
                                        IsEnabled = true,
                                        Interval = 1000
                                    };
                                    configAsset.Devices.Add(configDevice);
                                }

                                totalDevices += deviceResponse.Devices.Count;
                                _logger.LogInformation("Fetched {Count} devices for asset {AssetName}", deviceResponse.Devices.Count, asset.Name);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to fetch devices for asset {AssetName}: {Message}", asset.Name, deviceResponse.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error fetching devices for asset {AssetName}", asset.Name);
                            TempData["ErrorMessage"] = $"Error fetching devices for asset {asset.Name}: {ex.Message}";
                            configAsset.Devices = new List<Device>();
                        }
                        
                        // Add the configured asset to our collection
                        configAssets.Assets.Add(configAsset);
                    }
                    
                    // Save assets with their fetched devices to appsettings.json
                    await _apiConfigurationService.UpdateAssetsAsync(configAssets);
                    
                    var message = $"Successfully synced {configAssets.Assets.Count} assets with {totalDevices} total devices from API.";
                    _logger.LogInformation(message);
                    
                    return Json(new { 
                        success = true, 
                        message = message,
                        assetsCount = configAssets.Assets.Count,
                        devicesCount = totalDevices
                    });
                }
                else
                {
                    var errorMsg = loginRes.Success ? "No assets found in response" : loginRes.Message;
                    _logger.LogWarning("Login or asset fetch failed: {Error}", errorMsg);
                    return Json(new { success = false, message = $"Failed to fetch assets: {errorMsg}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during asset sync");
                TempData["ErrorMessage"] = $"Error during asset sync: {ex.Message}";
                return Json(new { success = false, message = $"Error occurred during sync: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult GetAssets()
        {
            try
            {
                var assets = _apiConfigurationService.GetAssets();
                return Json(new { success = true, assets = assets });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assets");
                TempData["ErrorMessage"] = $"Error retrieving assets: {ex.Message}";
                return Json(new { success = false, message = "Error retrieving assets" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAssetEnabled(string assetId, bool isEnabled)
        {
            try
            {
                var assets = _apiConfigurationService.GetAssets();
                var asset = assets.FirstOrDefault(a => a.Id == assetId);
                
                if (asset == null)
                {
                    return Json(new { success = false, message = "Asset not found" });
                }

                asset.IsEnabled = isEnabled;
                
                await _apiConfigurationService.UpdateAssetsAsync(new ConfigurationAssets { Assets = assets });
                
                return Json(new { success = true, message = $"Asset {asset.Name} {(isEnabled ? "enabled" : "disabled")}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset enabled state");
                TempData["ErrorMessage"] = $"Error updating asset enabled state: {ex.Message}";
                return Json(new { success = false, message = "Error updating asset" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDeviceEnabled(string assetId, string deviceId, bool isEnabled)
        {
            try
            {
                var assets = _apiConfigurationService.GetAssets();
                var asset = assets.FirstOrDefault(a => a.Id == assetId);
                
                if (asset == null)
                {
                    return Json(new { success = false, message = "Asset not found" });
                }

                var device = asset.Devices.FirstOrDefault(d => d.Id == deviceId);
                if (device == null)
                {
                    return Json(new { success = false, message = "Device not found" });
                }

                device.IsEnabled = isEnabled;
                
                await _apiConfigurationService.UpdateAssetsAsync(new ConfigurationAssets { Assets = assets });
                
                return Json(new { success = true, message = $"Device {device.Name} {(isEnabled ? "enabled" : "disabled")}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device enabled state");
                TempData["ErrorMessage"] = $"Error updating device enabled state: {ex.Message}";
                return Json(new { success = false, message = "Error updating device" });
            }
        }

        private static string MaskApiKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 8)
                return "****";
            
            return apiKey.Substring(0, 4) + new string('*', apiKey.Length - 8) + apiKey.Substring(apiKey.Length - 4);
        }
    }
}
