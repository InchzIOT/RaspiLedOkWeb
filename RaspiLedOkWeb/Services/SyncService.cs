using JsonDataClass;
using Microsoft.Extensions.Options;
using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using System.Text;
using System.Text.Json;

namespace RaspiLedOkWeb.Services
{
    public class SyncService : ISyncService
    {
        private readonly ILogger<SyncService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly HttpClient _httpClient;
        private readonly IApiConfigurationService _configService;

        public SyncService(
            ILogger<SyncService> logger, 
            IWebHostEnvironment environment,
            HttpClient httpClient,
            IApiConfigurationService configService)
        {
            _environment = environment;
            _logger = logger;
            _httpClient = httpClient;
            _configService = configService;
        }

        public async Task<JsonAuthResponse> Login(JsonAuth jsonAuth)
        {
            return await Login(jsonAuth.Username, jsonAuth.Password);
        }

        public async Task<JsonAuthResponse> Login(string username, string password)
        {
            try
            {
                var config = _configService.GetConfiguration();
                string endpoint = "/doggoconsole/Authentication/Login";
                var fullUrl = $"{config.Endpoint.TrimEnd('/')}{endpoint}";

                var loginData = new JsonAuth
                {
                    Username = username,
                    Password = password
                };

                var jsonContent = JsonSerializer.Serialize(loginData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Attempting login to {Url}", fullUrl);

                var response = await _httpClient.PostAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var authResponse = JsonSerializer.Deserialize<JsonAuthResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Login successful for user {Username}", username);
                    return authResponse ?? new JsonAuthResponse { Success = false, Message = "Invalid response format" };
                }
                else
                {
                    _logger.LogWarning("Login failed with status {StatusCode}: {Response}", response.StatusCode, responseContent);
                    return new JsonAuthResponse
                    {
                        Success = false,
                        Message = $"Login failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", username);
                return new JsonAuthResponse
                {
                    Success = false,
                    Message = $"Login error: {ex.Message}"
                };
            }
        }

        public async Task<JsonDeviceListResponse> GetDeviceListByAsset(int assetId)
        {
            try
            {
                var config = _configService.GetConfiguration();
                string endpoint = $"/doggoconsole/SmartPole/GetDeviceListByAsset?assetId={assetId}";
                var fullUrl = $"{config.Endpoint.TrimEnd('/')}{endpoint}";

                _logger.LogInformation("Fetching device list for asset {AssetId} from {Url}", assetId, fullUrl);

                var response = await _httpClient.GetAsync(fullUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var deviceResponse = JsonSerializer.Deserialize<JsonDeviceListResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return deviceResponse ?? new JsonDeviceListResponse { Success = false, Message = "Invalid response format" };
                }
                else
                {
                    _logger.LogWarning("Device list fetch failed with status {StatusCode}: {Response}", response.StatusCode, responseContent);
                    return new JsonDeviceListResponse
                    {
                        Success = false,
                        Message = $"Fetch failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching device list for asset {AssetId}", assetId);
                return new JsonDeviceListResponse
                {
                    Success = false,
                    Message = $"Fetch error: {ex.Message}"
                };
            }
        }
    }
}
