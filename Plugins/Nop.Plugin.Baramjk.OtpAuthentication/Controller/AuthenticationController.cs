using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Api;
using Nop.Plugin.Baramjk.OtpAuthentication.Services.Abstractions;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Controller
{
    [Route("FrontendApi")]
    public class AuthenticationController : BaseBaramjkApiController
    {
        private readonly IAuthorizationUserService _authorizationUserService;

        public AuthenticationController(IAuthorizationUserService authorizationUserService)
        {
            _authorizationUserService = authorizationUserService;
        }

        [HttpPost("otp/authenticate/gettoken")]
        public virtual async Task<IActionResult> GetToken([FromBody] AuthenticateCustomerRequest request)
        {
            var response = await _authorizationUserService.AuthenticateAsync(request);

            if (response == null)
                return ApiResponseFactory.BadRequest("Incorrect login information");

            return ApiResponseFactory.Success(response);
        }
    }
}