using Nop.Plugin.Baramjk.OtpAuthentication.Consts;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Models.Api
{
    public class SendOtpApiModel
    {
        public string PhoneNumber { get; set; }
        
        public string Email { get; set; }
        
        public OtpType OtpType { get; set; }
    }
}