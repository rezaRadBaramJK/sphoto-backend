
using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.OtpAuthentication.ExternalServices.Interfaces
{
    public interface IOtpProviderService
    {
        Task SendOtpAsync(string phoneNumber, string otp);
    }
}