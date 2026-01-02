using Doggo.Data.Models;
using JsonDataClass;
using JsonDataClass.HHT;
using Microsoft.Extensions.Options;
using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using RestSharp;
using System.Text;
using System.Text.Json;

namespace RaspiLedOkWeb.Services
{
    public class SyncService(
        ILogger<SyncService> logger,
        IWebHostEnvironment environment,
        HttpClient httpClient,
        IApiConfigurationService configService) : ISyncService
    {
        #region Properties
        private readonly ILogger<SyncService> _logger = logger;
        private readonly IWebHostEnvironment _environment = environment;
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfigurationService _configService = configService;
        public static RestClient restClient;
        public static RestClientOptions restOptions;
        private static string _bearerToken;
        private static string _lastApiUrl;
        #endregion

        #region Initialization
        private void InitializeRestClient()
        {
            var config = _configService.GetConfiguration();
            var currentApiUrl = config.ApiUrl.TrimEnd('/');
            
            // Check if RestClient needs to be reinitialized due to URL change
            //if (restClient == null || _lastApiUrl != currentApiUrl)
            //{
                // Dispose old client if it exists
                restClient?.Dispose();
                
                restOptions = new RestClientOptions(currentApiUrl)
                {
                    ThrowOnAnyError = false,
                    Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
                };
                restClient = new RestClient(restOptions);
                _lastApiUrl = currentApiUrl;
                
                // Clear bearer token when URL changes - user will need to login again
                if (_lastApiUrl != currentApiUrl && !string.IsNullOrEmpty(_bearerToken))
                {
                    _bearerToken = null;
                    _logger.LogInformation("RestClient reinitialized with new URL: {ApiUrl}. Bearer token cleared.", currentApiUrl);
                }
                else
                {
                    _logger.LogInformation("RestClient initialized with URL: {ApiUrl}", currentApiUrl);
                }
                
                // Re-apply bearer token if available
                if (!string.IsNullOrEmpty(_bearerToken))
                {
                    restClient.AddDefaultHeader("Authorization", $"Bearer {_bearerToken}");
                }
            //}
        }

        private void SetBearerToken(string token)
        {
            _bearerToken = token;
            if (restClient != null && !string.IsNullOrEmpty(token))
            {
                restClient.AddDefaultHeader("Authorization", $"Bearer {token}");
            }
        }

        #endregion

        #region Authentication
        public async Task<JsonAuthResponse> AutoLogin()
        {
            var config = _configService.GetConfiguration();
            return await Login(config.Username, CryptoHelper.PreDecrypt(config.Password));
        }

        public async Task<JsonAuthResponse> Login(JsonAuth jsonAuth)
        {
            return await Login(jsonAuth.Username, jsonAuth.Password);
        }

        public async Task<JsonAuthResponse> Login(string username, string password)
        {
            JsonAuthResponse resp = new JsonAuthResponse();
            try
            {
                // Initialize RestClient if not already done
                InitializeRestClient();

                string endpoint = "/doggoconsole/Authentication/Login";

                RestRequest request = new RestRequest(endpoint, Method.Post);

                var loginData = new JsonAuth
                {
                    Username = username,
                    Password = CryptoHelper.PreDecrypt(password),
                    ClientVer = "string",
                    DeviceId = "string",
                    DeviceType = "string",
                    DeviceVer = "string",
                    DeviceManufacturer = "string",
                    DeviceModel = "string",
                    DeviceName = "string"
                };
                request.AddJsonBody(loginData);
                RestResponse restResponse = restClient.Execute(request);
                if(restResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    resp = JsonSerializer.Deserialize<JsonAuthResponse>(restResponse.Content, new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Store the bearer token if login was successful
                    if (resp.Success && resp.AuthHeader != null && !string.IsNullOrEmpty(resp.AuthHeader.Token))
                    {
                        SetBearerToken(resp.AuthHeader.Token);
                        _logger.LogInformation("Bearer token set successfully for user {Username}", username);
                    }
                }
                else
                {
                    resp.Success = false;
                    resp.Message = $"{restResponse.StatusCode} {restResponse.StatusDescription}: {restResponse.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", username);
                resp.Success = false;
                resp.Message = ex.Message;
            }
            return resp;
        }
        #endregion

        #region Devices
        public async Task<JsonDeviceListResponse> GetDeviceListByAsset(int assetId)
        {
            JsonDeviceListResponse resp = new JsonDeviceListResponse();
            resp.Success = false;
            try
            {
                // Ensure RestClient is initialized
                InitializeRestClient();

                var config = _configService.GetConfiguration();
                string endpoint = $"/doggoconsole/SmartPole/GetDeviceListByAsset";
                var fullUrl = $"{config.ApiUrl.TrimEnd('/')}{endpoint}";

                RestRequest request = new RestRequest(endpoint, Method.Get);
                request.AddParameter("assetId", assetId);
                RestResponse restResponse = restClient.Execute(request);

                if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    resp = JsonSerializer.Deserialize<JsonDeviceListResponse>(restResponse.Content, new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else if (restResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    resp.Message = "Unauthorized";
                }
                else if (restResponse.StatusCode == 0)
                {
                    resp.Message = "Timeout";
                }
                else
                {
                    resp.Message = restResponse.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }
            return resp;
        }
        #endregion

        #region Telemetry
        public async Task<AirSensorModel> GetAirSensorLatestDataByDeviceIdAsync(int deviceId)
        {
            AirSensorModel resp = new AirSensorModel();
            resp.Success = false;
            try
            {
                // Ensure RestClient is initialized
                InitializeRestClient();

                var config = _configService.GetConfiguration();
                string endpoint = $"/doggoconsole/TBData/GetAirSensorDataByDeviceId";
                var fullUrl = $"{config.ApiUrl.TrimEnd('/')}{endpoint}";

                RestRequest request = new RestRequest(endpoint, Method.Get);
                request.AddParameter("deviceId", deviceId);
                RestResponse restResponse = restClient.Execute(request);

                if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    resp = JsonSerializer.Deserialize<AirSensorModel>(restResponse.Content, new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else if (restResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    resp.Message = "Unauthorized";
                }
                else if (restResponse.StatusCode == 0)
                {
                    resp.Message = "Timeout";
                }
                else
                {
                    resp.Message = restResponse.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }
            return resp;
        }

        public async Task<PoleSensorModel> GetAirAndWaterSensorLatestDataByDeviceIdAsync(int airId, int waterId)
        {
            PoleSensorModel resp = new PoleSensorModel();
            resp.Success = false;
            try
            {
                // Ensure RestClient is initialized
                InitializeRestClient();

                var config = _configService.GetConfiguration();
                string endpoint = $"/doggoconsole/TBData/GetPoleSensorDataByDeviceId";
                var fullUrl = $"{config.ApiUrl.TrimEnd('/')}{endpoint}";

                RestRequest request = new RestRequest(endpoint, Method.Get);
                request.AddParameter("airDeviceId", airId);
                request.AddParameter("waterDeviceId", waterId);
                RestResponse restResponse = restClient.Execute(request);

                if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    resp = JsonSerializer.Deserialize<PoleSensorModel>(restResponse.Content, new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else if (restResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    resp.Message = "Unauthorized";
                }
                else if (restResponse.StatusCode == 0)
                {
                    resp.Message = "Timeout";
                }
                else
                {
                    resp.Message = restResponse.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }
            return resp;
        }
        #endregion

    }
}
