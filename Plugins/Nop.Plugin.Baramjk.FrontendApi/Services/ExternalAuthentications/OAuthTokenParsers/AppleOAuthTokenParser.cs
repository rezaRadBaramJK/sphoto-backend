using Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.Validators;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.OAuthTokenParsers
{
    public class AppleOAuthTokenParser : BaseOAuthTokenParser
    {
        public AppleOAuthTokenParser(IdTokenValidatorService idTokenValidatorService)
            : base(idTokenValidatorService)
        {
        }

        protected override OAuthProvider AuthProvider => OAuthProvider.Apple;
    }
}