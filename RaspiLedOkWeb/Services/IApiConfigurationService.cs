using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using System.Text.Json;

namespace RaspiLedOkWeb.Services
{
    internal interface IApiConfigurationService
    {
        ApiConfiguration GetConfiguration();
        Task UpdateConfigurationAsync(ApiConfiguration configuration);
        bool ValidateConfiguration(ApiConfiguration configuration);
    }
}