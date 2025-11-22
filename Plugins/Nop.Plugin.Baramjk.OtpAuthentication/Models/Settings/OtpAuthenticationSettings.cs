using Nop.Core.Configuration;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Types;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Models.Settings
{
    public class OtpAuthenticationSettings : ISettings
    {
        public string Message { get; set; }
        public int TokenLifetimeSeconds { get; set; }
        public int RefreshTokenLifetimeSeconds { get; set; }
        public bool AllowRegisterationAsVendor { get; set; }
        public bool DontRegisterIfUserNotExists { get; set; }

        public bool ShowOtpCodeInResponse { get; set; }

        public SendMethodType SendMethod { get; set; } = SendMethodType.Sms;
    }
}