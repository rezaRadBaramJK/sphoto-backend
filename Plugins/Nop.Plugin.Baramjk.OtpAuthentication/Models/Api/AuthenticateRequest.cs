
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Models.Api
{
    public abstract class AuthenticateRequest : BaseDto
    {
        public string RefreshToken { get; set; }

        public string PhoneNumber { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}