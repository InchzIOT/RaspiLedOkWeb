using System.Text.Json.Serialization;

namespace RaspiLedOkWeb.Models
{
    // Shape of a response from https://api.waqi.info/feed/{city}/?token={token}
    // On success, "status" is "ok" and "data" is an AqicnData object.
    // On failure, "status" is "error" and "data" is a plain error string instead.
    public class AqicnEnvelope
    {
        public string Status { get; set; } = string.Empty;
    }

    public class AqicnResponse
    {
        public string Status { get; set; } = string.Empty;
        public AqicnData? Data { get; set; }
    }

    public class AqicnErrorResponse
    {
        public string Status { get; set; } = string.Empty;
        public string? Data { get; set; }
    }

    public class AqicnData
    {
        public double Aqi { get; set; }
        public AqicnCity? City { get; set; }
        public Dictionary<string, AqicnIaqiValue>? Iaqi { get; set; }
    }

    public class AqicnCity
    {
        public string Name { get; set; } = string.Empty;
    }

    public class AqicnIaqiValue
    {
        public double? V { get; set; }
    }
}
