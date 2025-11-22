using System.Threading.Tasks;
using Nop.Plugin.Baramjk.OtpAuthentication.Consts;
using Nop.Plugin.Baramjk.OtpAuthentication.Domain;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Services.Abstractions
{
    public interface IMobileOtpService
    {
        public Task InsertAsync(MobileOtp entity);
        public Task UpdateAsync(MobileOtp entity);
        public Task DeleteAsync(MobileOtp entity);
        public Task<MobileOtp> FindByPhoneNumber(string phoneNumber, OtpType otpType);
    }
}