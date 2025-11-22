using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;
using Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications;

namespace Nop.Plugin.Baramjk.FrontendApi.Models
{
    public class AuthenticateRequest : BaseDto
    {
        public string IdToken { get; set; }

        public string AccessToken { get; set; }

        public string Device { get; set; }

        public UserInfoModel UserInfo { get; set; }
    }
}