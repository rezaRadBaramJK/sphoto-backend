using Nop.Core.Configuration;
using Nop.Plugin.Baramjk.FrontendApi.Models.Types;

namespace Nop.Plugin.Baramjk.FrontendApi
{
    /// <summary>
    ///     Represents settings of the Web API Frontend
    /// </summary>
    public class FrontendApiSettings : ISettings
    {
        public bool DeveloperMode { get; set; }
        public string SecretKey { get; set; }

        public int TokenLifetimeDays { get; set; }
        public int TokenLifetimeMinutes { get; set; }
        public string GoogleIOSClientId { get; set; }
        public string AppleIOSClientId { get; set; }
        public string FaceBookIOSClientId { get; set; }

        public string GoogleAndroidClientId { get; set; }
        public string AppleAndroidClientId { get; set; }
        public string FaceBookAndroidClientId { get; set; }

        public string GoogleWebClientId { get; set; }
        public string AppleWebClientId { get; set; }
        public string FaceBookWebClientId { get; set; }
        public bool DontRequireAddress { get; set; }

        public string UploadFileSupportedTypes { get; set; } = string.Empty;
        
        public double UploadFileMaxSize { get; set; } = 2;
        
        public string CustomFrontendBaseUrl { get; set; }
        
        public PasswordRecoveryStrategy PasswordRecoveryStrategy { get; set; }
    }
}