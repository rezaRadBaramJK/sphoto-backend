using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Exceptions.Auth;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Controllers;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Helpers;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Models;
using Nop.Plugin.Baramjk.FrontendApi.Services;
using Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications;
using AuthenticateRequest = Nop.Plugin.Baramjk.FrontendApi.Models.AuthenticateRequest;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    [Route(WebApiFrontendDefaults.ROUTE, Order = int.MaxValue)]
    [ApiExplorerSettings(GroupName = WebApiFrontendDefaults.AREA)]
    public class AuthenticateController : BaseNopWebApiController
    {
        #region Ctor

        public AuthenticateController(
            IAuthorizationUserService authorizationUserService,
            ExternalAuthenticationApiService externalAuthenticationApiService,
            FrontendApiSettings frontendApiSettings)
        {
            _authorizationUserService = authorizationUserService;
            _externalAuthenticationApiService = externalAuthenticationApiService;
            _frontendApiSettings = frontendApiSettings;
        }

        #endregion

     
        #region Fields

        private readonly IAuthorizationUserService _authorizationUserService;
        private readonly ExternalAuthenticationApiService _externalAuthenticationApiService;
        private readonly FrontendApiSettings _frontendApiSettings;

        #endregion

        #region Methods

        /// <summary>
        ///     Authenticate user
        /// </summary>
        /// <param name="request"></param>
        [Authorize(true)]
        [HttpPost]
        [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetToken([FromBody] AuthenticateCustomerRequest request)
        {
            var response = await _authorizationUserService.AuthenticateAsync(request);

            if (response == null)
                return ApiResponseFactory.BadRequest("Incorrect login information");

            return ApiResponseFactory.Success(response);
        }

        [Authorize(true)]
        [HttpPost]
        [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetTokenByGoogle([FromBody] AuthenticateRequest request)
        {
            return await AuthenticateByToken(request, OAuthProvider.Google);
        }

        [Authorize(true)]
        [HttpPost]
        [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetTokenByApple([FromBody] AuthenticateRequest request)
        {
            return await AuthenticateByToken(request, OAuthProvider.Apple);
        }

        [Authorize(true)]
        [HttpPost]
        [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetTokenByFacebook([FromBody] AuthenticateRequest request)
        {
            return await AuthenticateByToken(request, OAuthProvider.Facebook);
        }

        #endregion
        
        
           private async Task<IActionResult> AuthenticateByToken(AuthenticateRequest request, OAuthProvider provider)
        {
            var clientId = GetClientId(provider, request.Device);

            var signInModel = new SignInModel
            {
                Provider = provider,
                AccessToken = request.AccessToken,
                ClientId = clientId,
                IdToken = request.IdToken,
                UserInfo = request.UserInfo
            };
            try
            {
                var response = await _externalAuthenticationApiService.SignInAsync(signInModel);
                return response == null
                    ? ApiResponseFactory.BadRequest("Token is incorrect")
                    : ApiResponseFactory.Success(response);
            }
            catch (IncorrectInfoException e)
            {
                return ApiResponseFactory.BadRequest(e.Message);
            }
            catch (EmailAlreadyExistsException e)
            {
                return ApiResponseFactory.BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return ApiResponseFactory.InternalServerError(e.Message);
            }
            
        }

        private string GetClientId(OAuthProvider provider, string requestDevice)
        {
            var clientId = "";

            clientId = provider switch
            {
                OAuthProvider.Google => requestDevice switch
                {
                    "IOS" => _frontendApiSettings.GoogleIOSClientId,
                    "Android" => _frontendApiSettings.GoogleAndroidClientId,
                    "Web" => _frontendApiSettings.GoogleWebClientId,
                    _ => clientId
                },
                OAuthProvider.Apple => requestDevice switch
                {
                    "IOS" => _frontendApiSettings.AppleIOSClientId,
                    "Android" => _frontendApiSettings.AppleAndroidClientId,
                    "Web" => _frontendApiSettings.AppleWebClientId,
                    _ => clientId
                },
                OAuthProvider.Facebook => requestDevice switch
                {
                    "IOS" => _frontendApiSettings.FaceBookIOSClientId,
                    "Android" => _frontendApiSettings.FaceBookAndroidClientId,
                    "Web" => _frontendApiSettings.FaceBookWebClientId,
                    _ => clientId
                },
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
            };

            return clientId;
        }

    }
}