using Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.Validators;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.OAuthTokenParsers
{
    public class GoogleOAuthTokenParser : BaseOAuthTokenParser
    {
        public GoogleOAuthTokenParser(IdTokenValidatorService idTokenValidatorService)
            : base(idTokenValidatorService)
        {
        }

        protected override OAuthProvider AuthProvider => OAuthProvider.Google;
    }
}