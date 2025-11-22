using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.Validators;
using Nop.Services.Authentication.External;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.OAuthTokenParsers
{
    public abstract class BaseOAuthTokenParser
    {
        private readonly IdTokenValidatorService _idTokenValidatorService;

        protected BaseOAuthTokenParser(IdTokenValidatorService idTokenValidatorService)
        {
            _idTokenValidatorService = idTokenValidatorService;
        }

        protected abstract OAuthProvider AuthProvider { get; }

        public virtual async Task<OAuthTokenDataResult> GetOAuthTokenDataAsync(SignInModel model)
        {
            var validateResult = await ValidateIdToken(model.IdToken, model.ClientId);
            if (validateResult.IsSuccess == false)
                return new OAuthTokenDataResult
                {
                    ValidateResult = validateResult
                };

            var tokenData = ParsAsync(validateResult.Principal, model);

            return new OAuthTokenDataResult
            {
                ValidateResult = validateResult,
                OAuthTokenData = tokenData,
                IsSuccess = true
            };
        }

        protected virtual OAuthTokenData ParsAsync(ClaimsPrincipal principal, SignInModel model)
        {
            var parameters = new OAuthTokenData();

            parameters.ProviderName = AuthProvider.ToString();
            parameters.AccessToken = model.AccessToken;
            parameters.IdToken = model.IdToken;

            parameters.Email = principal.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value;

            parameters.ExternalIdentifier =
                principal.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            parameters.ExternalDisplayIdentifier =
                principal.FindFirst(claim => claim.Type is ClaimTypes.Name or "name")?.Value;

            parameters.Claims = principal.Claims.Select(claim =>
                new ExternalAuthenticationClaim(claim.Type, claim.Value)).ToList();

            parameters.FirstName = principal.FindFirst(claim => claim.Type == ClaimTypes.GivenName)?.Value;
            parameters.LastName = principal.FindFirst(claim => claim.Type == ClaimTypes.Surname)?.Value;
            parameters.Gender = principal.FindFirst(claim => claim.Type == ClaimTypes.Gender)?.Value;
            parameters.MobileNumber = principal.FindFirst(claim => claim.Type == ClaimTypes.MobilePhone)?.Value;
            parameters.Avatar = principal.FindFirst(claim => claim.Type is "avatar" or "picture")?.Value;

            var dateOfBirthValue = principal.FindFirst(claim => claim.Type == ClaimTypes.DateOfBirth)?.Value;
            if (string.IsNullOrEmpty(dateOfBirthValue) == false)
                parameters.Birthdate = DateTime.Parse(dateOfBirthValue);

            FillByUserInfo(model?.UserInfo, parameters);

            return parameters;
        }

        protected static void FillByUserInfo(UserInfoModel infoModel, OAuthTokenData parameters)
        {
            if (infoModel == null)
                return;

            if (string.IsNullOrEmpty(parameters.FirstName) && string.IsNullOrEmpty(infoModel.FirstName) == false)
                parameters.FirstName = infoModel.FirstName;

            if (string.IsNullOrEmpty(parameters.LastName) && string.IsNullOrEmpty(infoModel.LastName) == false)
                parameters.LastName = infoModel.LastName;

            if (string.IsNullOrEmpty(parameters.Gender) && string.IsNullOrEmpty(infoModel.Gender) == false)
                parameters.Gender = infoModel.Gender;

            if (string.IsNullOrEmpty(parameters.MobileNumber) && string.IsNullOrEmpty(infoModel.MobileNumber) == false)
                parameters.MobileNumber = infoModel.MobileNumber;

            if (string.IsNullOrEmpty(parameters.Avatar) && string.IsNullOrEmpty(infoModel.Avatar) == false)
                parameters.Avatar = infoModel.Avatar;

            if (parameters.Birthdate == null && infoModel.Birthdate != null)
                parameters.Birthdate = infoModel.Birthdate;
        }

        protected virtual async Task<ValidateResult> ValidateIdToken(string idToken, string clientId)
        {
            var validateResult = await _idTokenValidatorService.ValidateAsync(idToken, clientId, AuthProvider);
            return validateResult;
        }
    }
}