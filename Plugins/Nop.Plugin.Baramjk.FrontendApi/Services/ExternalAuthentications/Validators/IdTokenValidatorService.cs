using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.Validators
{
    public class IdTokenValidatorService
    {
        public async Task<ValidateResult> ValidateAsync(string idToken, string clientId, OAuthProvider provider)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = tokenHandler.ReadJwtToken(idToken);
                var validationParameters =
                    await GetTokenValidationParametersAsync(clientId, jwtSecurityToken, provider);
                var principal = tokenHandler.ValidateToken(idToken, validationParameters, out var _);

                return new ValidateResult
                {
                    IsSuccess = true,
                    Principal = principal
                };
            }
            catch (Exception exception)
            {
                return new ValidateResult
                {
                    IsSuccess = false,
                    Exception = exception
                };
            }
        }

        protected virtual async Task<TokenValidationParameters> GetTokenValidationParametersAsync
            (string clientId, JwtSecurityToken jwtSecurityToken, OAuthProvider oAuthProvider)
        {
            var publicKey = await GetSecurityKeyAsync(jwtSecurityToken, oAuthProvider);

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = GetValidIssuer(oAuthProvider),
                IssuerSigningKey = publicKey,
                ValidAudience = clientId,
                ValidateLifetime = true
            };

            return validationParameters;
        }

        protected virtual async Task<SecurityKey> GetSecurityKeyAsync(JwtSecurityToken jwtSecurityToken
            , OAuthProvider oAuthProvider)
        {
            var url = GetJwkUri(oAuthProvider);

            //todo: cache 
            using var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync(url);
            var content = await responseMessage.Content.ReadAsStringAsync();
            var jsonWebKeySet = new JsonWebKeySet(content);
            SecurityKey publicKey = jsonWebKeySet.Keys.FirstOrDefault(x => x.Kid == jwtSecurityToken.Header.Kid);

            return publicKey;
        }

        protected virtual string GetJwkUriDebug(OAuthProvider oAuthProvider)
        {
            switch (oAuthProvider)
            {
                case OAuthProvider.Google:
                    return "https://localhost:8001/certs/google.json";

                case OAuthProvider.Apple:
                    return "https://localhost:8001/certs/apple.json";

                case OAuthProvider.Facebook:
                    return "https://localhost:8001/certs/facebook.json";
                default:
                    throw new ArgumentOutOfRangeException(nameof(oAuthProvider), oAuthProvider, null);
            }
        }

        protected virtual string GetJwkUri(OAuthProvider oAuthProvider)
        {
            switch (oAuthProvider)
            {
                case OAuthProvider.Google:
                    return "https://www.googleapis.com/oauth2/v3/certs";

                case OAuthProvider.Apple:
                    return "https://appleid.apple.com/auth/keys";

                case OAuthProvider.Facebook:
                    return "https://www.facebook.com/.well-known/oauth/openid/jwks/";
                default:
                    throw new ArgumentOutOfRangeException(nameof(oAuthProvider), oAuthProvider, null);
            }
        }

        protected virtual string GetValidIssuer(OAuthProvider oAuthProvider)
        {
            switch (oAuthProvider)
            {
                case OAuthProvider.Google:
                    return "https://accounts.google.com";

                case OAuthProvider.Apple:
                    return "https://appleid.apple.com";

                case OAuthProvider.Facebook:
                    return "https://www.facebook.com";
                default:
                    throw new ArgumentOutOfRangeException(nameof(oAuthProvider), oAuthProvider, null);
            }
        }
    }
}