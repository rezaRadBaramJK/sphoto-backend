using Nop.Plugin.Baramjk.OtpAuthentication.Consts;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Models.Api
{
    public class ValidateOtpApiModel
    {
        public string PhoneNumber { get; set; }
        
        public string Email { get; set; }
        
        public string Otp { get; set; }
        
        public OtpType OtpType { get; set; }
        
        public bool AsVendor { get; set; }
        
        public string Password { get; set; }
    }
}