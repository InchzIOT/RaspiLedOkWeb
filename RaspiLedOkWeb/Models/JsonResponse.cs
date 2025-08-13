using RaspiLedOkWeb.Helpers;

namespace RaspiLedOkWeb.Models
{
    public class JsonLoginResponse
    {
        public bool success { get; set; }   
        public string token { get; set; }
        public string message { get; set; }
    }
}
