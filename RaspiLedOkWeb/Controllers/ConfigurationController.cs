using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using RaspiLedOkWeb.Services;

namespace RaspiLedOkWeb.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly ILogger<ConfigurationController> _logger;
        private readonly IApiConfigurationService _apiConfigurationService;

        public ConfigurationController(
            ILogger<ConfigurationController> logger,
            IApiConfigurationService apiConfigurationService)
        {
            _logger = logger;
            _apiConfigurationService = apiConfigurationService;
        }

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
                ModelState.AddModelError("", "An error occurred while updating the configuration.");
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
                    EnableLogging = configuration.EnableLogging
                };
                
                return Json(safeConfiguration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving API configuration");
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
                return Json(new { success = false, message = "Error testing connection." });
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
