using JsonDataClass;
using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using System.Text.Json;

namespace RaspiLedOkWeb.Services
{
    public interface ISyncService
    {
        Task<JsonAuthResponse> Login(JsonAuth jsonAuth);
        Task<JsonAuthResponse> Login(string username, string password);
        Task<JsonDeviceListResponse> GetDeviceListByAsset(int assetId);
    }
}