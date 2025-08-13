using JsonDataClass;
using Microsoft.Extensions.Options;
using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using System.Text.Json;

namespace RaspiLedOkWeb.Services
{
    public class SyncService : ISyncService
    {
        private readonly ILogger<SyncService> _logger;
        private readonly IWebHostEnvironment _environment;

        public SyncService(ILogger<SyncService> logger, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
        }

        public Task<JsonAuthResponse> Login(JsonAuth jsonAuth)
        {
            string endpoint = "/doggoconsole/Authentication/Login";

            throw new NotImplementedException();
        }


        public Task<JsonAuthResponse> Login(string username, string password)
        {
            JsonAuth jsonAuth = new JsonAuth();
            jsonAuth.Username = username;
            jsonAuth.Password = password;
            string endpoint = "/doggoconsole/Authentication/Login";

            throw new NotImplementedException();
        }
        public Task<JsonDeviceListResponse> GetDeviceListByAsset(int assetId)
        {
            string endpoint = $"/doggoconsole/SmartPole/GetDeviceListByAsset?assetId={assetId}";

            throw new NotImplementedException();
        }
    }
}
