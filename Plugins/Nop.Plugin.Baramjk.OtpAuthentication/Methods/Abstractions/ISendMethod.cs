using System.Threading.Tasks;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Methods;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Types;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Methods.Abstractions
{
    public interface ISendMethod
    {
        SendMethodType Type { get; }

        Task SendAsync(SendOtpParams sendParams);
    }
}