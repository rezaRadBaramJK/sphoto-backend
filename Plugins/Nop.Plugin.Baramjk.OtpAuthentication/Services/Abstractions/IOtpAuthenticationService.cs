using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Api;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Services.Abstractions
{
    public interface IOtpAuthenticationService
    {
        Task<SendOtpResponseApiModel> SendChangePhoneNumberOtp(SendOtpApiModel model, Customer customer);
        Task<SendOtpResponseApiModel> SendOtpAsync(SendOtpApiModel model, string oldPhoneNumber = null);
        Task<ValidateOtpResponseApiModel> ValidateOtpAsync(ValidateOtpApiModel model);
        Task<SendOtpResponseApiModel> SendChangePasswordOtpAsync(SendOtpApiModel model);
    }
}