using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Framework.Models
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