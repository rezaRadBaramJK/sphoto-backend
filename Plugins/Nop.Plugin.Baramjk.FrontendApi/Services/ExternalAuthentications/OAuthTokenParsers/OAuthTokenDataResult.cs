using Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.Validators;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.OAuthTokenParsers
{
    public class OAuthTokenDataResult
    {
        public ValidateResult ValidateResult { get; set; }

        public OAuthTokenData OAuthTokenData { get; set; }

        public bool IsSuccess { get; set; }
    }
}