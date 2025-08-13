using RaspiLedOkWeb.Helpers;

namespace RaspiLedOkWeb.Models
{
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
