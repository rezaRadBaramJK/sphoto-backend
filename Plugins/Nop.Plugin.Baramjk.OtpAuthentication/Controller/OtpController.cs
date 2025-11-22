using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.OtpAuthentication.Consts;
using Nop.Plugin.Baramjk.OtpAuthentication.Helpers;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Api;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Settings;
using Nop.Plugin.Baramjk.OtpAuthentication.Services.Abstractions;
using ApiResponseFactory = Nop.Plugin.Baramjk.OtpAuthentication.Models.ApiResponseFactory;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Controller
{
    [Route("FrontendApi")]
    public class OtpController : BaseBaramjkApiController
    {
        private readonly IOtpAuthenticationService _otpAuthenticationService;
        private readonly IWorkContext _workContext;
        private readonly OtpAuthenticationSettings _otpAuthenticationSettings;

        public OtpController(IOtpAuthenticationService otpAuthenticationService, IWorkContext workContext,
            OtpAuthenticationSettings otpAuthenticationSettings)
        {
            _otpAuthenticationService = otpAuthenticationService;
            _workContext = workContext;
            _otpAuthenticationSettings = otpAuthenticationSettings;
        }

        [HttpPost("otp/send")]
        public async Task<IActionResult> SendOtpAsync([FromBody] SendOtpApiModel model)
        {
            model.PhoneNumber = PhoneNumberHelper.RemoveFirstBelongAtTheBeginningOfThePhoneNumber(model.PhoneNumber);


            SendOtpResponseApiModel result;
            if (model.OtpType == OtpType.RegisterOrLogin)
            {
                result = await _otpAuthenticationService.SendOtpAsync(model);
            }
            else if (model.OtpType == OtpType.ChangePhone)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                result = await _otpAuthenticationService.SendChangePhoneNumberOtp(model, customer);
            }
            else
            {
                result = await _otpAuthenticationService.SendChangePasswordOtpAsync(model);
            }

            if (result.Success == false)
            {
                return ApiResponseFactory.BadRequest("Failed to send otp", result.ErrorMessages);
            }

            if (_otpAuthenticationSettings.ShowOtpCodeInResponse == false)
            {
                result.Otp = null;
                return ApiResponseFactory.Success(result);
            }

            return ApiResponseFactory.Success(result);
        }

        [HttpPost("otp/validate")]
        public async Task<IActionResult> ValidateOtpAsync([FromBody] ValidateOtpApiModel model)
        {
            if (model.AsVendor)
            {
                if (!_otpAuthenticationSettings.AllowRegisterationAsVendor)
                    return ApiResponseFactory.BadRequest("Registration as vendor is not allowed.");
            }

            model.PhoneNumber = PhoneNumberHelper.RemoveFirstBelongAtTheBeginningOfThePhoneNumber(model.PhoneNumber);
            var result = await _otpAuthenticationService.ValidateOtpAsync(model);

            return result.Success
                ? ApiResponseFactory.Success(result)
                : ApiResponseFactory.BadRequest("Otp validation failed", result.ErrorMessages);
        }
    }
}