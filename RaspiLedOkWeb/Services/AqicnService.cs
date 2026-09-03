using Doggo.Data.Models;
using RaspiLedOkWeb.Models;
using System.Text.Json;

namespace RaspiLedOkWeb.Services
{
    // Fetches air quality data from the AQICN / World Air Quality Index Project
    // (https://aqicn.org, API at https://api.waqi.info) as an alternative to the
    // ThingsBoard-backed SyncService, mapped into the same AirSensorModel shape so
    // the rest of the app (views, gauge, thresholds) doesn't need to know the source.
    public class AqicnService(
        HttpClient httpClient,
        ILogger<AqicnService> logger) : IAqicnService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<AqicnService> _logger = logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<AirSensorModel> GetLatestDataAsync(string city, string apiToken)
        {
            var result = new AirSensorModel { Success = false };

            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(apiToken))
            {
                result.Error = "AQICN city or API token is not configured";
                return result;
            }

            try
            {
                var url = $"https://api.waqi.info/feed/{Uri.EscapeDataString(city)}/?token={Uri.EscapeDataString(apiToken)}";
                var json = await _httpClient.GetStringAsync(url);

                using var document = JsonDocument.Parse(json);
                var status = document.RootElement.GetProperty("status").GetString();

                if (!string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase))
                {
                    // On failure "data" is a plain error string (e.g. "Unknown station", "Invalid key")
                    var errorMessage = document.RootElement.TryGetProperty("data", out var errorData)
                        ? errorData.GetString()
                        : "Unknown AQICN error";
                    result.Error = errorMessage;
                    result.Message = errorMessage;
                    _logger.LogWarning("AQICN request failed for city {City}: {Error}", city, errorMessage);
                    return result;
                }

                var response = JsonSerializer.Deserialize<AqicnResponse>(json, JsonOptions);
                var data = response?.Data;
                if (data == null)
                {
                    result.Error = "AQICN response did not contain data";
                    return result;
                }

                var iaqi = data.Iaqi ?? new Dictionary<string, AqicnIaqiValue>(StringComparer.OrdinalIgnoreCase);

                result.OverallAPI = data.Aqi.ToString();
                result.Temperature = GetIaqiValue(iaqi, "t");
                result.Humidity = GetIaqiValue(iaqi, "h");
                result.Pm25 = GetIaqiValue(iaqi, "pm25");
                result.Pm10 = GetIaqiValue(iaqi, "pm10");
                result.O3 = GetIaqiValue(iaqi, "o3");
                result.Co = GetIaqiValue(iaqi, "co");
                result.No2 = GetIaqiValue(iaqi, "no2");
                result.So2 = GetIaqiValue(iaqi, "so2");
                result.Success = true;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AQICN data for city {City}", city);
                result.Error = ex.Message;
                result.Message = ex.Message;
                return result;
            }
        }

        private static string? GetIaqiValue(Dictionary<string, AqicnIaqiValue> iaqi, string key)
        {
            return iaqi.TryGetValue(key, out var entry) && entry.V.HasValue
                ? entry.V.Value.ToString()
                : null;
        }
    }
}
