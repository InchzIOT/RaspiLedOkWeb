using Doggo.Data.Models;

namespace RaspiLedOkWeb.Services
{
    public interface IAqicnService
    {
        Task<AirSensorModel> GetLatestDataAsync(string city, string apiToken);
    }
}
