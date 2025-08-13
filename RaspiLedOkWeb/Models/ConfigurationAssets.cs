using RaspiLedOkWeb.Helpers;

namespace RaspiLedOkWeb.Models
{
    public class ConfigurationAssets
    {
        public List<Asset> Assets { get; set; }
    }

    public class Asset
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool IsEnabled { get; set; }
        public int Interval { get; set; }
        public List<Device> Devices { get; set; }
    }

    public class Device
    {
        public string Name { get; set; }
        public string Id {  set; get; }
        public bool IsEnabled { get; set; }
        public int Interval { get; set; }
    }

}
