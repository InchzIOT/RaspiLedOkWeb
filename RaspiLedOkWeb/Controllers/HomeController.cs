using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using RaspiLedOkWeb.Services;

namespace RaspiLedOkWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISyncService _syncService;
        private readonly IApiConfigurationService _apiConfigurationService;

        public HomeController(ILogger<HomeController> logger, ISyncService syncService, IApiConfigurationService apiConfigurationService)
        {
            _logger = logger;
            _syncService = syncService;
            _apiConfigurationService = apiConfigurationService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var configRes = _apiConfigurationService.GetConfiguration();
                ConfigurationAssets configAssets = new ConfigurationAssets();

                // Check if we have valid configuration
                if (string.IsNullOrEmpty(configRes.Username) || string.IsNullOrEmpty(configRes.Password))
                {
                    ViewBag.Message = "Please configure API credentials first.";
                    ViewBag.MessageType = "warning";
                    return View();
                }

                _logger.LogInformation("Attempting to login and fetch assets");
                
                // Login using configured credentials
                var loginRes = await _syncService.Login(configRes.Username, configRes.GetDecryptedPassword());
                
                if (loginRes.Success && loginRes.AuthHeader?.Assets != null)
                {
                    _logger.LogInformation("Login successful, found {Count} assets", loginRes.AuthHeader.Assets.Count);

                    var totalDevices = 0;

                    // Fetch devices for each asset
                    foreach (var asset in loginRes.AuthHeader.Assets)
                    {
                        Asset configAsset = new Asset()
                        {
                            Name = asset.Name,
                            Id = asset.AssetId.ToString(),
                            IsEnabled = true,
                            Interval = 1000
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
                                        Name = device.DeviceName, // Changed from DeviceName to Name
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
                            configAsset.Devices = new List<Device>();
                        }
                        
                        // Add the configured asset to our collection
                        configAssets.Assets.Add(configAsset);
                    }
                    
                    // Save assets with their fetched devices to appsettings.json
                    await _apiConfigurationService.UpdateAssetsAsync(configAssets);
                    
                    ViewBag.Message = $"Successfully synced {configAssets.Assets.Count} assets with {totalDevices} total devices from API.";
                    ViewBag.MessageType = "success";
                    ViewBag.Assets = configAssets.Assets;
                }
                else
                {
                    var errorMsg = loginRes.Success ? "No assets found in response" : loginRes.Message;
                    _logger.LogWarning("Login or asset fetch failed: {Error}", errorMsg);
                    ViewBag.Message = $"Failed to fetch assets: {errorMsg}";
                    ViewBag.MessageType = "danger";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Home/Index");
                ViewBag.Message = $"Error occurred: {ex.Message}";
                ViewBag.MessageType = "danger";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RefreshAssets()
        {
            try
            {
                var configRes = _apiConfigurationService.GetConfiguration();
                
                if (string.IsNullOrEmpty(configRes.Username) || string.IsNullOrEmpty(configRes.Password))
                {
                    return Json(new { success = false, message = "API credentials not configured" });
                }

                var loginRes = await _syncService.Login(configRes.Username, configRes.GetDecryptedPassword());
                
                if (loginRes.Success && loginRes.AuthHeader?.Assets != null)
                {
                    ConfigurationAssets configAssets = new ConfigurationAssets();
                    var totalDevices = 0;
                    
                    // Fetch devices for each asset
                    foreach (var asset in loginRes.AuthHeader.Assets)
                    {
                        Asset configAsset = new Asset()
                        {
                            Name = asset.Name,
                            Id = asset.AssetId.ToString(),
                            IsEnabled = true,
                            Interval = 1000
                        };

                        try
                        {
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
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error fetching devices for asset {AssetName} during refresh", asset.Name);
                            configAsset.Devices = new List<Device>();
                        }
                        
                        configAssets.Assets.Add(configAsset);
                    }
                    
                    await _apiConfigurationService.UpdateAssetsAsync(configAssets);
                    
                    return Json(new { 
                        success = true, 
                        message = $"Successfully refreshed {configAssets.Assets.Count} assets with {totalDevices} devices",
                        assetsCount = configAssets.Assets.Count,
                        devicesCount = totalDevices
                    });
                }
                else
                {
                    return Json(new { success = false, message = loginRes.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing assets");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
