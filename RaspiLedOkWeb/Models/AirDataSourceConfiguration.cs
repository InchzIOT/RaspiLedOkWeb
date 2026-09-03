namespace RaspiLedOkWeb.Models
{
    public enum AirDataProvider
    {
        ThingsBoard,
        Aqicn
    }

    public class AqicnConfiguration
    {
        // Token issued by the AQICN (aqicn.org / World Air Quality Index Project) data platform.
        public string ApiToken { get; set; } = string.Empty;

        // City slug as used in AQICN feed URLs (e.g. "shanghai", "beijing"), or "here" to
        // auto-detect the city from the server's IP address.
        public string City { get; set; } = "here";
    }

    public class AirDataSourceConfiguration
    {
        public const string SectionName = "AirDataSource";

        public AirDataProvider Provider { get; set; } = AirDataProvider.ThingsBoard;
        public AqicnConfiguration Aqicn { get; set; } = new AqicnConfiguration();
    }
}
