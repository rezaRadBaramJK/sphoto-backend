using Nop.Core.Configuration;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Models.Settings
{
    /// <summary>
    ///     Represents settings of the Web API Frontend
    /// </summary>
    public class FrontendApiSettings : ISettings
    {
        public bool DeveloperMode { get; set; }
        public string SecretKey { get; set; }

        public int TokenLifetimeDays { get; set; }
        public string GoogleIOSClientId { get; set; }
        public string AppleIOSClientId { get; set; }
        public string FaceBookIOSClientId { get; set; }

        public string GoogleAndroidClientId { get; set; }
        public string AppleAndroidClientId { get; set; }
        public string FaceBookAndroidClientId { get; set; }

        public string GoogleWebClientId { get; set; }
        public string AppleWebClientId { get; set; }
        public string FaceBookWebClientId { get; set; }
    }
}