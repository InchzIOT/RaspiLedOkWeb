namespace RaspiLedOkWeb.Models
{
    public class ApiConfiguration
    {
        public const string SectionName = "ApiConfiguration";
        public string Endpoint { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class Assets
    {
        public const string SectionName = "Assets";
        public EntityAttributes[] AssetList { get; set; }
    }

    public class Devices
    {
        public const string SectionName = "Devices";
        public EntityAttributes[] DeviceList { get; set; }
    }

    public class EntityAttributes
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool IsEnabled { get; set; } // To show in this dashboard or not
        public int Interval { get; set; }    // In seconds
    }
}
