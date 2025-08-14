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
        #endregion

        #region Authentication
        public async Task<JsonAuthResponse> Login(JsonAuth jsonAuth)
        {
            return await Login(jsonAuth.Username, jsonAuth.Password);
        }

        public async Task<JsonAuthResponse> Login(string username, string password)
        {
            JsonAuthResponse resp = new JsonAuthResponse();
            try
            {
                var config = _configService.GetConfiguration();
                string endpoint = "/doggoconsole/Authentication/Login";
                var fullUrl = $"{config.ApiUrl.TrimEnd('/')}{endpoint}";

                RestRequest request = new RestRequest(endpoint, Method.Post);

                var loginData = new JsonAuth
                {
                    Username = username,
                    Password = password
                };
                request.AddJsonBody(loginData);
                RestResponse restResponse = await restClient.ExecuteAsync(request);
                if(restResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    resp = JsonSerializer.Deserialize<JsonAuthResponse>(restResponse.Content, new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });
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
        #endregion

    }
}
