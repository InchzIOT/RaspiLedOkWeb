using RaspiLedOkWeb.Helpers;

namespace RaspiLedOkWeb.Models
{
    public class ScreenConfiguration
    {
        public string Width { get; set; }
        public string Height { get; set; }
    }

    public class ApiConfiguration
    {
        public const string SectionName = "ApiConfiguration";
        public string Endpoint { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        private string _encryptedPassword;
        public string Password
        {
            get => _encryptedPassword; // Returns encrypted value
            set => _encryptedPassword = string.IsNullOrEmpty(value)
                ? string.Empty
                : CryptoHelper.PreEncrypt(value);
        }
        public int TimeoutSeconds { get; set; } = 30;
        public bool EnableLogging { get; set; } = true;
        public string GetDecryptedPassword()
        {
            return string.IsNullOrEmpty(_encryptedPassword)
                ? string.Empty
                : CryptoHelper.PreDecrypt(_encryptedPassword);
        }
    }

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
        public string Id { set; get; }
        public bool IsEnabled { get; set; }
        public int Interval { get; set; }
    }
}
