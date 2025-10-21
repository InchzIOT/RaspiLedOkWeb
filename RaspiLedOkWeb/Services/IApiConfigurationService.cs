using JsonDataClass;
using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using System.Text.Json;

namespace RaspiLedOkWeb.Services
{
    public interface IApiConfigurationService
    {
        ApiConfiguration GetConfiguration();
        Task UpdateConfigurationAsync(ApiConfiguration configuration);
        bool ValidateConfiguration(ApiConfiguration configuration);
        List<Asset> GetAssets();
        Task UpdateAssetsAsync(ConfigurationAssets configurationAssets);
        AppSettings GetAppSettings();
    }
}