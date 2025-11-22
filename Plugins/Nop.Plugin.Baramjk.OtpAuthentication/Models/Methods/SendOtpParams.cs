namespace Nop.Plugin.Baramjk.OtpAuthentication.Models.Methods
{
    public class SendOtpParams
    {
        public string PhoneNumber { get; set; }

        public string Message { get; set; }
        
        public string OtpCode { get; set; }
        
        public string Email { get; set; }
        
    }
}