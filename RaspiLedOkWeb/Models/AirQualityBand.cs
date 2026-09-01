namespace RaspiLedOkWeb.Models
{
    public class AirQualityBand
    {
        // Upper bound (inclusive) of this band. Null means "no upper bound" (last/catch-all band).
        public int? Max { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Malaysia DOE API bands. Used whenever "AirQuality:Bands" is missing from appsettings.json.
        public static List<AirQualityBand> Defaults => new()
        {
            new AirQualityBand
            {
                Max = 50,
                Status = "Good",
                Color = "#00BFFF",
                Message = "Air quality is <span style='font-weight:bold' id='newsApi'>Good</span>. Please enjoy your outdoor activities!"
            },
            new AirQualityBand
            {
                Max = 100,
                Status = "Moderate",
                Color = "#32CD32",
                Message = "Air quality is <span style='font-weight:bold' id='newsApi'>Moderate</span>. Safe for most, but children & elderly should limit long outdoor exposure."
            },
            new AirQualityBand
            {
                Max = 200,
                Status = "Unhealthy",
                Color = "#FFFF00",
                Message = "Limit outdoor exposure & wear a protective mask, as the air is now unhealthy, especially for sensitive groups"
            },
            new AirQualityBand
            {
                Max = 300,
                Status = "Very Unhealthy",
                Color = "#FF8C00",
                Message = "Everyone should now avoid outdoor exertion & wear an N95 mask if leaving the indoors is necessary."
            },
            new AirQualityBand
            {
                Max = 400,
                Status = "Hazardous",
                Color = "#FF0000",
                Message = "Remain indoors to avoid serious health risks from hazardous air; an N95 mask is critical for any essential exposure."
            },
            new AirQualityBand
            {
                Max = null,
                Status = "Hazardous",
                Color = "#FF0000",
                Message = "Air quality is <span style='font-weight:bold' id='newsApi'>Hazardous</span>. High health risk. Strictly stay indoors. Wear an N95 mask if you must go out."
            }
        };
    }
}
